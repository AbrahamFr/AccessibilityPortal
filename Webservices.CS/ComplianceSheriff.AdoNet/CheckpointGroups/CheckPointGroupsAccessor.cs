using ComplianceSheriff.CheckpointGroups;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ComplianceSheriff.Checkpoints;
using ComplianceSheriff.Scans;
using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.Configuration;
using Microsoft.Extensions.Options;
using ComplianceSheriff.Licensing;

namespace ComplianceSheriff.AdoNet.CheckpointGroups
{
    public class CheckpointGroupsAccessor : ICheckpointGroupsAccessor
    {
        #region "SQL Queries"

             #region "CheckpointGroupings Query"
                public static readonly string sqlCheckpointGroupings = @"

                            SELECT DISTINCT CONVERT(VARCHAR, ScanId) ParentId, CheckpointGroupId ChildID 
                            INTO #TempCheckPointGroupAssoc 
                            FROM ScanCheckpointGroups

                            SELECT * FROM #TempCheckPointGroupAssoc

                            SELECT * FROM CheckpointGroupCheckpoints

                            SELECT * FROM CheckpointGroupSubGroups

                            SELECT 
                                   CheckpointId,
                                   Number,
                                   LongDescription,
                                   ShortDescription
                            FROM Checkpoints

                            SELECT 
                                   CheckpointGroupId,
                                   ShortDescription,
                                   LongDescription
                            FROM CheckpointGroups
        
                            SELECT * FROM ScanGroupScans

                            SELECT * FROM ScanGroupSubGroups

                            SELECT 
                                    ScanGroupId,
                                    DisplayName
                            FROM ScanGroups

                            Select s.ScanId,
	                               s.DisplayName,
	                               scpg.CheckpointGroupId
                            FROM Scans s
                              LEFT OUTER JOIN ScanCheckpointGroups scpg
                                ON scpg.ScanId = s.ScanId";
             #endregion

             #region "CheckpointGroupList Query"
                public static string sqlCheckpointGroupListQry = @"
                    DECLARE @LicensedCheckpointGroups TABLE (
                        CheckpointGroupId nvarchar(32),
                        ShortDescription varchar(max)
                    )

                    INSERT INTO @LicensedCheckpointGroups 
                    SELECT * FROM udfGetLicensedCheckpointGroups(@LicensedModules)

                    SELECT * FROM @LicensedCheckpointGroups lcpg
                        INNER JOIN [dbo].[udfCheckpointGroupsByPermission](@UserGroupId, @PermissionType) cpgbp
                        ON lcpg.CheckpointGroupId = cpgbp.CheckpointGroupId
                    ORDER BY ShortDescription";
            #endregion

             #region "CheckpointGroupsByQuery"

                private readonly string checkpointGroupsByQuery = @"
                      DECLARE @LicensedCheckpointGroups TABLE (
                          CheckpointGroupId nvarchar(32),
                          ShortDescription varchar(max)
                      )

                      INSERT INTO @LicensedCheckpointGroups 
                      SELECT * FROM udfGetLicensedCheckpointGroups(@LicensedModules)

                      Select  DISTINCT 
		                      lcpg.CheckpointGroupId
                      INTO #CheckpointIds
                      FROM @LicensedCheckpointGroups lcpg
                      INNER JOIN [dbo].[ScanCheckpointGroups] scpg WITH (NOLOCK)
	                     ON lcpg.CheckpointGroupId = scpg.CheckpointGroupId
					  INNER JOIN [dbo].[udfScansByPermission](@UserGroupId, 'Scan') sp
					    ON scpg.ScanId = sp.ScanId
					  LEFT OUTER JOIN [dbo].[ScanGroupScans] sgs
                        ON sgs.ScanId = sp.ScanId
					  LEFT OUTER JOIN [dbo].[ScanGroups] sg WITH (NOLOCK)
					    ON sg.ScanGroupId = sgs.ScanGroupId
                      LEFT OUTER JOIN [dbo].[udfScanGroupsByPermission](@UserGroupId, 'ScanGroup') sgp
                        ON sgp.ScanGroupId = sg.ScanGroupId
                      INNER JOIN [dbo].[udfCheckpointGroupsByPermission](@UserGroupId, 'CheckpointGroup') cpgp
                         ON cpgp.CheckpointGroupId = lcpg.CheckpointGroupId 
                      WHERE (@ScanGroupId IS NULL OR sg.ScanGroupId IN (Select ScanGroupId FROM [dbo].[udfScanGroupHierarchyByScanGroupId] (@ScanGroupId)))
                        AND (@ScanId IS NULL OR scpg.ScanId = @ScanId)
                        AND (@CheckpointGroupId IS NULL OR lcpg.CheckpointGroupId = @CheckpointGroupId)                      

                      ;WITH cteCheckPointGroupHierarchy AS
						(
							SELECT cpg.CheckpointGroupId, CAST(NULL AS nvarchar(32)) AS ParentId
							FROM CheckpointGroups cpg
							WHERE cpg.CheckpointGroupId IN (Select * FROM #CheckpointIds)

							UNION ALL

							SELECT SubgroupId, cpgsg.CheckpointGroupId AS ParentId
							FROM CheckpointGroupSubGroups cpgsg
							INNER JOIN cteCheckPointGroupHierarchy c
								ON cpgsg.CheckpointGroupId = c.CheckpointGroupId
						)
                        SELECT cpg.CheckpointGroupId, ShortDescription FROM cteCheckPointGroupHierarchy cpgh
                        INNER JOIN CheckpointGroups cpg
                        ON cpgh.CheckpointGroupId = cpg.CheckpointGroupId
                        ORDER BY cpg.ShortDescription

						DROP TABLE #CheckpointIds";
            #endregion

        #endregion

        private readonly IConnectionManager connection;
        private readonly ConfigurationOptions _configOptions;
        private readonly ILicensingService _licensingService;
        private readonly ILogger<CheckpointGroupsAccessor> _logger;

        public CheckpointGroupsAccessor(IConnectionManager connection,
                                        IOptions<ConfigurationOptions> options,
                                        ILogger<CheckpointGroupsAccessor> logger,
                                        ILicensingService licensingService)
        {
            _configOptions = options.Value;
            this.connection = connection;
            _licensingService = licensingService;
            _logger = logger;
        }

        public async Task<List<CheckpointGroupListItem>> GetCheckpointGroupList(int userGroupId, string organizationId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlCheckpointGroupListQry,
                        new Dictionary<string, Action<DbParameter>>
                        {
                            { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                            { "@PermissionType", p => p.DbType = System.Data.DbType.String },
                            { "@LicensedModules", p => p.DbType = System.Data.DbType.String }
                        },
                        System.Data.CommandType.Text
                    );

            var checkpointGroupList = new List<CheckpointGroupListItem>();

            var licensedModules = _licensingService.GetLicensedModuleString(organizationId, _configOptions);

            using (var command = await commandBuilder.BuildFrom(connection,
                  new Dictionary<string, object>
                  {
                      { "@UserGroupId", userGroupId },
                      { "@PermissionType", "CheckpointGroup" },
                      { "@LicensedModules",  licensedModules}
                  }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        checkpointGroupList.Add(new CheckpointGroupListItem
                        {
                            CheckpointGroupId = reader["CheckpointGroupId"].ToString(),
                            ShortDescription = reader["ShortDescription"].ToString()
                        });
                    }
                }
            }

            return checkpointGroupList;
        }

        public async Task<CheckpointGroupings> GetCheckpointGroupings(CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlCheckpointGroupings,
                        new Dictionary<string, Action<DbParameter>>
                        { },
                        System.Data.CommandType.Text
                    );

            var checkpointGroupings = new CheckpointGroupings();

            using (var command = await commandBuilder.BuildFrom(connection,
                 new Dictionary<string, object>
                 { }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        checkpointGroupings.CheckpointGroupScanAssociations.Add(new CheckpointGroupScanAssociation
                        {
                            CheckpointGroupId = reader["ChildId"].ToString(),
                            ScanId = reader["ParentId"].ToString()
                        });
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        checkpointGroupings.CheckpointGroupCheckpoints.Add(new CheckpointGroupCheckpoint
                        {
                            CheckpointGroupId = reader["CheckpointGroupId"].ToString(),
                            CheckpointId = reader["CheckpointId"].ToString()
                        });
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        checkpointGroupings.CheckpointGroupSubGroups.Add(new CheckpointGroupSubGroup
                        {
                            CheckpointGroupId = reader["CheckpointGroupId"].ToString(),
                            SubGroupId = reader["SubGroupId"].ToString()
                        });
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        checkpointGroupings.Checkpoints.Add(new Checkpoint
                        {
                            CheckpointId = reader["CheckpointId"].ToString(),
                            Number = reader["Number"].ToString(),
                            LongDescription = reader["LongDescription"].ToString(),
                            ShortDescription = reader["ShortDescription"].ToString()
                        });
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var checkpointGroup = new CheckpointGroup
                        {
                            CheckpointGroupId = reader["CheckpointGroupId"].ToString(),
                            ShortDescription = reader["ShortDescription"].ToString(),
                            LongDescription = reader["LongDescription"].ToString(),
                        };

                        checkpointGroup.Checkpoints.AddRange(checkpointGroupings.CheckpointGroupCheckpoints.Where(g => g.CheckpointGroupId == checkpointGroup.CheckpointGroupId).Select(g => g.CheckpointId).ToArray());
                        checkpointGroup.Subgroups.AddRange(checkpointGroupings.CheckpointGroupSubGroups.Where(sg => sg.CheckpointGroupId == checkpointGroup.CheckpointGroupId).Select(g => g.SubGroupId).ToArray());
                        checkpointGroupings.CheckpointGroups.Add(checkpointGroup);
                    }

                    //Possible Break

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var scanGroupScans = new ScanGroupScan
                        {
                            ScanGroupId = Convert.ToInt32(reader["ScanGroupId"].ToString()),
                            ScanId = Convert.ToInt32(reader["ScanId"].ToString())
                        };

                        checkpointGroupings.ScanGroupScans.Add(scanGroupScans);
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var scanGroupSubGroup = new ScanGroupSubGroup
                        {
                            ScanGroupId = Convert.ToInt32(reader["ScanGroupId"].ToString()),
                            SubGroupId = Convert.ToInt32(reader["SubGroupId"].ToString())
                        };

                        checkpointGroupings.ScanGroupSubGroups.Add(scanGroupSubGroup);
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var scanGroup = new ScanGroup
                        {
                            ScanGroupId = Convert.ToInt32(reader["ScanGroupId"].ToString()),
                            DisplayName = reader["DisplayName"].ToString()
                        };

                        var scanArray = checkpointGroupings.ScanGroupScans
                                                           .Where(sg => sg.ScanGroupId == scanGroup.ScanGroupId)
                                                           .Select(sg => sg.ScanId).ToList()
                                                           .ConvertAll<string>(x => x.ToString()).ToArray();

                        var subGroupArray = checkpointGroupings.ScanGroupSubGroups
                                                                                   .Where(sg => sg.ScanGroupId == scanGroup.ScanGroupId)
                                                                                   .Select(sg => sg.SubGroupId).ToList()
                                                                                   .ConvertAll<string>(x => x.ToString()).ToArray();

                        scanGroup.Scans.AddRange(scanArray);
                        scanGroup.Subgroups.AddRange(subGroupArray);
                        checkpointGroupings.ScanGroups.Add(scanGroup);
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var scanByCheckpointGroup = new ScanByCheckpointGroup
                        {
                            ScanId = reader["ScanId"].ToString(),
                            DisplayName = reader["DisplayName"].ToString(),
                            CheckpointGroupId = reader["CheckpointGroupId"].ToString()
                        };

                        checkpointGroupings.ScansByCheckpointGroup.Add(scanByCheckpointGroup);
                    }
                }
            }

            return checkpointGroupings;
        }

        public async Task<List<CheckpointGroupListItem>> GetCheckpointGroupsBy(int userGroupId, string organizationId, int? scanId, int? scanGroupId, string checkpointGroupId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(checkpointGroupsByQuery,
                        new Dictionary<string, Action<DbParameter>>
                        {
                             { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                             { "@PermissionType", p => p.DbType = System.Data.DbType.String },
                             { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
                             { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                             { "@CheckpointGroupId", p => p.DbType = System.Data.DbType.String },
                             { "@LicensedModules", p => p.DbType = System.Data.DbType.String }
                        },
                        System.Data.CommandType.Text
                    );

            var checkpointGroupsList = new List<CheckpointGroupListItem>();

            var licensedModules = _licensingService.GetLicensedModuleString(organizationId, _configOptions);

            using (var command = await commandBuilder.BuildFrom(connection,
                 new Dictionary<string, object>
                 {
                     { "@UserGroupId", userGroupId },
                     { "@PermissionType", "CheckpointGroup"},
                     { "@ScanId", scanId == null ? (object)DBNull.Value : scanId },
                     { "@ScanGroupId", scanGroupId == null ? (object)DBNull.Value : scanGroupId },
                     { "@CheckpointGroupId", checkpointGroupId == null ? (object)DBNull.Value : checkpointGroupId },
                     { "@LicensedModules",  licensedModules}
                 }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        checkpointGroupsList.Add(new CheckpointGroupListItem
                        {
                            CheckpointGroupId = reader["CheckpointGroupId"].ToString(),
                            ShortDescription = reader["ShortDescription"].ToString()
                        });
                    }
                }
            }

            return checkpointGroupsList;
        }
    }
}
