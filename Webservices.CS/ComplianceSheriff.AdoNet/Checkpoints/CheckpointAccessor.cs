using ComplianceSheriff.Checkpoints;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ComplianceSheriff.Licensing;
using ComplianceSheriff.Configuration;
using Microsoft.Extensions.Options;

namespace ComplianceSheriff.AdoNet.Checkpoints
{
    public class CheckpointAccessor : ICheckpointAccessor
    {
        private readonly IConnectionManager connection;
        private readonly ConfigurationOptions _configOptions;
        private readonly ILicensingService _licensingService;
        private readonly ILogger<CheckpointAccessor> _logger;

        #region "SQL Query"

            #region "Licensed Checkpoints Query"

            static readonly string licensedCheckpointsQry = @"

					    DECLARE @LicensedModulesTbl TABLE (
					       ModuleName nvarchar(32)
					    )

                        INSERT INTO @LicensedModulesTbl 
					    SELECT * FROM udf_STRING_SPLIT(@LicensedModules, ',')

					    SELECT 
                            LEFT(cp.CheckpointId, CHARINDEX('_', cp.CheckpointId)-1) AS Module,
                            cp.CheckpointId, 
						    LTRIM(cp.ShortDescription) AS CheckpointName,
						    cpg.CheckpointGroupId,
						    cpg.ShortDescription AS CheckpointGroupName
					    FROM Checkpoints cp
					    LEFT OUTER JOIN CheckpointGroupCheckpoints cpgcp
					      ON cpgcp.CheckpointId = cp.CheckpointId
					    LEFT OUTER JOIN CheckpointGroups cpg
					      ON cpg.CheckpointGroupId = cpgcp.CheckpointGroupId
                        WHERE LEFT(cp.CheckpointId, CHARINDEX('_', cp.CheckpointId)-1) IN (SELECT * FROM @LicensedModulesTbl)
                        ORDER BY LTRIM(cp.ShortDescription)";
            #endregion
    
            #region "Checkpoint Failure Query"
            static readonly string top10FailuresText = @"
                    
                        --GET ORDERED LIST BY LatestRunDate DESC
                        Select  ROW_NUMBER() OVER (ORDER BY RunDate DESC, ScanGroupRunId DESC) AS RN, * INTO #ScanGroupRunOrdered
							                                            FROM reporting.ScanGroupRuns WITH (NOLOCK)
												                        Where ScanGroupId = @ScanGroupId
                                                                          AND ScheduledScan = @ScheduledScan
												                          AND RunDate IS NOT NULL
												                        ORDER BY RunDate DESC

		                    DECLARE @RunOrder TABLE (
			                    RunOrder int,
			                    RunId int
		                    )

                            -- GET ALL RUN IDS WITH ORDER NUMBER
		                    INSERT INTO @RunOrder
		                    SELECT RN, RunId FROM #ScanGroupRunOrdered o
		                      INNER JOIN Runs r
		                       ON r.ScanGroupRunId = o.ScanGroupRunId
		                    WHERE RN <= 3
		                    ORDER BY RN

		                    ;WITH PageRunsResults([URL], CheckId, Result, [Priority], RunId, RunOrder)
		                    AS
		                    (
			                    SELECT  p.Url,
					                    res.CheckId,
					                    res.Result,
					                    res.[Priority],
					                    r.RunId,
					                    ro.RunOrder
			                    FROM Pages p WITH (NOLOCK)
				                    INNER JOIN Runs r WITH (NOLOCK)
					                    ON p.RunId = r.RunId
				                    INNER JOIN Results res WITH (NOLOCK)
					                    ON res.PageId = p.PageId
					                    AND res.RunId = r.RunId
				                    INNER JOIN @RunOrder ro
					                    ON ro.RunId = r.RunId
			                    WHERE IsPage = 1
				                    AND Result = 0
		                    )
		                    SELECT * INTO #PageRunsResults FROM PageRunsResults

	                        SELECT TOP 10 Number,
				                            [ShortDescription], 
				                            (SELECT COUNT(1) FROM #PageRunsResults i WHERE i.CheckId = o.CheckId AND RunOrder = 1)  As CurrentFailures,
						                    (SELECT COUNT(1) FROM #PageRunsResults i WHERE i.CheckId = o.CheckId AND RunOrder = 2)  As OneRunBackFailures,
						                    (SELECT COUNT(1) FROM #PageRunsResults i WHERE i.CheckId = o.CheckId AND RunOrder = 3)  As TwoRunsBackFailures,
				                            (SELECT COUNT(DISTINCT URL) FROM #PageRunsResults i WHERE i.CheckId = o.CheckId AND RunOrder = 1) AS PagesImpacted,
				                            (SELECT COUNT(1) FROM #PageRunsResults i WHERE i.CheckId = o.CheckId AND i.[Priority] = 1 AND RunOrder = 1) AS Priority1Failures
	                        FROM Checkpoints cp
	                        INNER JOIN #PageRunsResults o
	                            ON o.CheckId = cp.CheckpointId
	                        GROUP BY Number, CheckId, [ShortDescription]
	                        ORDER BY CurrentFailures DESC

                    DROP TABLE #ScanGroupRunOrdered
                    DROP TABLE #PageRunsResults";

            #endregion

        #endregion


        public CheckpointAccessor(IConnectionManager connection,
                                  IOptions<ConfigurationOptions> options,
                                  ILogger<CheckpointAccessor> logger,
                                  ILicensingService licensingService)
        {
            this.connection = connection;
            _configOptions = options.Value;
            _licensingService = licensingService;
            _logger = logger;
        }

        public async Task<IEnumerable<CheckpointFailure>> GetTop10CheckpointFailures(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true)
        {
            CommandBuilder commandBuilder = new CommandBuilder(top10FailuresText,
                   new Dictionary<string, Action<DbParameter>>
                   {
                        { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@ScheduledScan", p => p.DbType = System.Data.DbType.Boolean }
                   },
                   System.Data.CommandType.Text
               );

            using (var command = await commandBuilder.BuildFrom(connection,
                      new Dictionary<string, object>
                      {
                        { "@ScanGroupId", scanGroupId },
                        { "@ScheduledScan", scheduledScan },
                      }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    var checkpointFailures = new List<CheckpointFailure>();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var checkpointFailure = new CheckpointFailure
                        {
                            CheckpointId = reader["Number"].ToString(),
                            Description = reader["ShortDescription"].ToString(),
                            CurrentFailures = Convert.ToInt32(reader["CurrentFailures"].ToString()),
                            OneRunBackFailures = Convert.ToInt32(reader["OneRunBackFailures"].ToString()),
                            TwoRunsBackFailures = Convert.ToInt32(reader["TwoRunsBackFailures"].ToString()),
                            Priority1Failures = Convert.ToInt32(reader["Priority1Failures"].ToString()),
                            PagesImpacted = Convert.ToInt32(reader["PagesImpacted"].ToString())

                        };

                        checkpointFailures.Add(checkpointFailure);
                    }

                    return checkpointFailures;
                }
            }
        }

        public async Task<IEnumerable<CheckpointCheckpointGroupItem>> GetLicensedCheckpoints(string organizationId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(licensedCheckpointsQry,
                   new Dictionary<string, Action<DbParameter>>
                   {
                        { "@LicensedModules", p => p.DbType = System.Data.DbType.String }
                   },
                   System.Data.CommandType.Text
               );

            var licensedModules = _licensingService.GetLicensedModuleString(organizationId, _configOptions);

            using (var command = await commandBuilder.BuildFrom(connection,
                       new Dictionary<string, object>
                       {
                         { "@LicensedModules",  licensedModules}
                       }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    var checkpoints = new List<CheckpointCheckpointGroupItem>();
                    

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var checkpointCheckpointGroupItem = new CheckpointCheckpointGroupItem
                        {
                            Module = reader["Module"].ToString(),
                            CheckpointId = reader["CheckpointId"].ToString(),
                            CheckpointName = reader["CheckpointName"].ToString(),
                            CheckpointGroupId = reader["CheckpointGroupId"].ToString(),
                            CheckpointGroupName = reader["CheckpointGroupName"].ToString()
                        };

                        checkpoints.Add(checkpointCheckpointGroupItem);
                    }

                    return checkpoints;
                }
            }
        }
    }
}
