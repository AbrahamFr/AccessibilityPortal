using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.Enums;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ComplianceSheriff.Scans;

namespace ComplianceSheriff.AdoNet.ScanGroups
{
    public class ScanGroupScanAccessor : IScanGroupScanAccessor
    {
        private readonly IConnectionManager connection;
        private readonly ILogger<ScanGroupScanAccessor> _logger;

        #region "SQL Queries"

            #region "Checkpoint Performance Query"

                public static readonly string scanGroupCheckPointPerformanceText = @"SELECT TOP 1 * FROM reporting.ScanGroupRuns 
                                                                                      WHERE ScanGroupID = @ScanGroupId 
                                                                                        AND ScheduledScan = @ScheduledScan
                                                                                      ORDER BY RunDate DESC, ScanGroupRunId DESC";
            #endregion

            #region "Page Performance Query"

            static readonly string scanGroupPagePerformanceText = @"SELECT TOP 1 * FROM reporting.ScanGroupRuns 
                                                                     Where ScangroupId = @ScanGroupId
                                                                     AND ScheduledScan = @ScheduledScan
                                                                     ORDER BY RunDate DESC, ScanGroupRunId DESC";

 
            #endregion
                      
            #region "ScanGroup History"
            static readonly string scangroupHistoryText = @"SELECT * FROM
                                                            (SELECT TOP 12 * FROM reporting.ScanGroupRuns
                                                                            Where ScanGroupId = @ScanGroupId
                                                                            AND ScheduledScan = @ScheduledScan
                                                                            AND RunDate IS NOT NULL
                                                                            ORDER BY RunDate DESC) AS ScanGroupRuns
                                                            ORDER BY RunDate";
            #endregion

            #region "Top 10 Page Failures"
            static readonly string top10PageFailuresText = @"

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

	                    SELECT TOP 10 URL,
			                    (SELECT COUNT(CheckId) FROM #PageRunsResults i WHERE i.Url = o.URL AND Result = 0 AND RunOrder = 1) As CurrentCheckpointFailures,
								(SELECT COUNT(CheckId) FROM #PageRunsResults i WHERE i.Url = o.URL AND Result = 0 AND RunOrder = 2) As OneRunBackCheckpointFailures,
								(SELECT COUNT(CheckId) FROM #PageRunsResults i WHERE i.Url = o.URL AND Result = 0 AND RunOrder = 3) As TwoRunsBackCheckpointFailures,
			                    (SELECT COUNT(CheckId) FROM #PageRunsResults i WHERE i.Url = o.URL AND Result = 0 AND Priority = 1 AND RunOrder = 1) As Priority1Failures,
			                    (SELECT COUNT(CheckId) FROM #PageRunsResults i WHERE i.Url = o.URL AND Result = 0 AND Priority = 2 AND RunOrder = 1) As Priority2Failures,
			                    (SELECT COUNT(CheckId) FROM #PageRunsResults i WHERE i.Url = o.URL AND Result = 0 AND Priority = 3 AND RunOrder = 1) As Priority3Failures	
	                    FROM #PageRunsResults o	    
	                    WHERE Result = 0
	                    GROUP BY [URL]
	                    ORDER BY CurrentCheckpointFailures DESC

	                    DROP TABLE #ScanGroupRunOrdered
	                    DROP TABLE #PageRunsResults";
        #endregion

            #region "Performance By Scan"
            static readonly string performanceByScan = @"

                DECLARE @ScanGroupRunId int

                SET @ScanGroupRunId = (Select TOP 1 ScanGroupRunId 
						                FROM reporting.ScanGroupRuns
                                        WHERE ScanGroupId = @ScanGroupId
                                        AND ScheduledScan = @ScheduledScan
						                ORDER BY RunDate DESC)

                SELECT r.ScanGroupId,
                       p.PageId,
	                   p.Url,
                       r.RunId,
	                   r.ScanId,
	                   CheckId,
	                   Result
                INTO #TempData
                FROM Pages p WITH (NOLOCK)
                  INNER JOIN Runs r WITH (NOLOCK)
                    ON p.RunId = r.RunId
                  INNER JOIN Results res WITH (NOLOCK)
                    ON res.RunId = r.RunId
                   AND res.PageId = p.PageId
                WHERE ScanGroupRunId = @ScanGroupRunId
                  AND p.IsPage = 1

                SELECT 
                       s.ScanId,
                       DisplayName, 
                       COUNT(DISTINCT URL) As PagesScanned,
	                   COUNT(DISTINCT CheckId) As ScannedCheckpoints,
                       (SELECT COUNT(DISTINCT CheckId) 
			                FROM #TempData tempData1 
			                WHERE NOT EXISTS(SELECT 1 FROM #TempData tempData2
			                                  WHERE tempData1.CheckId = tempData2.CheckId
							                    AND tempData2.ScanId = tempData1.ScanId
								                AND Result = 0)
			                  AND tempData1.ScanId = s.ScanId
			                ) AS PassedCheckpoints,
                       (SELECT COUNT(DISTINCT CheckId) 
			                FROM #TempData tempData1 
			                WHERE EXISTS(SELECT 1 FROM #TempData tempData2
			                                  WHERE tempData1.CheckId = tempData2.CheckId
							                    AND tempData2.ScanId = tempData1.ScanId
								                AND Result = 0)
			                  AND tempData1.ScanId = s.ScanId
			                ) AS FailedCheckpoints
                INTO #TempResults
                FROM Scans s WITH (NOLOCK)
		          INNER JOIN dbo.udfScansByPermission(@UserGroupId, @PermissionType) sbp
		            ON sbp.ScanId = s.ScanId
                  INNER JOIN #TempData tdata
                    ON s.ScanId = tdata.ScanId
                GROUP BY s.DisplayName, s.ScanId

                SELECT *, 
                       CONVERT(DECIMAL(5,2), 100 * (CAST(FailedCheckpoints as float) / CAST(ScannedCheckpoints as float))) As CheckpointFailurePercent
                FROM #TempResults
                ORDER BY CheckpointFailurePercent DESC

                DROP TABLE #TempData
                DROP TABLE #TempResults";

            #endregion

            #region "Not Used"
        #region "Scan Group Metrics"
        //static readonly string scanGroupMetricsText = @"               
        //            -- GET LATEST RunId FROM EACH Scan in ScanGroup
        //            ;WITH LatestRuns(ScanId, RunId)
        //            AS
        //            (
        //                SELECT ScanId, MAX(RunId) As RunId
        //                  FROM Runs r WITH (NOLOCK)
        //                Where  r.Finished BETWEEN @StartDate AND @FinishDate
        //                  AND r.ScanGroupId = @ScanGroupId
        //                  AND Status = 2
        //                  AND ScanId > 0
        //                GROUP BY ScanId
        //            )
        //            SELECT * INTO #LatestRuns FROM LatestRuns

        //            ; WITH PageRunsResults(PageId, RunId, [Url], [Started], [Finished], ScanGroupId, CheckId, Result)
        //            AS
        //            (
        //                SELECT p.PageId,
        //                        r.RunId,
        //                        p.[Url],
        //                        r.[Started],
        //                        r.[Finished],
        //                        r.ScanGroupId,
        //                        res.CheckId,
        //                        res.Result
        //                FROM Pages p WITH (NOLOCK)
        //                  INNER JOIN Runs r WITH (NOLOCK)
        //                    ON p.RunId = r.RunId
        //                  INNER JOIN Results res WITH (NOLOCK)
        //                    ON res.PageId = p.PageId
        //                   AND res.RunId = r.RunId
        //                WHERE r.RunId IN (Select #LatestRuns.RunId FROM #LatestRuns)
        //               AND IsPage = 1
        //            )
        //            SELECT * INTO #PageRunsResults FROM PageRunsResults

        //            SELECT
        //                @ScanGroupId AS ScanGroupId,
        //                (
        //                    (SELECT COUNT(DISTINCT[URL]) FROM #PageRunsResults)
        //             ) AS TotalScans,
        //             (
        //                 SELECT COUNT(1)
        //                    FROM
        //                    (SELECT DISTINCT URL
        //                      FROM #PageRunsResults AS o
        //                WHERE NOT EXISTS(SELECT 1 FROM #PageRunsResults i
        //                   WHERE i.PageId = o.PageId
        //                                       AND i.RunId = o.RunId
        //                                       AND Result = 0)) As PassedScans
        //             ) AS PassedScans,
        //             (		
        //                 SELECT COUNT(1)
        //                    FROM
        //                    (SELECT DISTINCT URL
        //                      FROM #PageRunsResults AS o
        //                WHERE EXISTS(SELECT 1 FROM #PageRunsResults i
        //                   WHERE i.PageId = o.PageId
        //                                       AND i.RunId = o.RunId
        //                                       AND Result = 0)) As FailedScans
        //             ) As FailedScans,
        //             (
        //              Select COUNT(DISTINCT CheckId) As Total FROM #PageRunsResults
        //             ) AS TotalCheckPointsScanned,
        //             (
        //                 SELECT COUNT(1)
        //                    FROM
        //                    (Select DISTINCT CheckId FROM #PageRunsResults o
        //               WHERE NOT EXISTS(SELECT 1 FROM #PageRunsResults i
        //                   WHERE i.CheckId = o.CheckId
        //                                       AND Result = 0)) As CheckpointsPassed
        //             ) As CheckpointsPassed,
        //             (
        //                 SELECT COUNT(1)
        //                    FROM
        //                    (Select DISTINCT CheckId FROM #PageRunsResults o
        //               WHERE EXISTS(SELECT 1 FROM #PageRunsResults i
        //                   WHERE i.CheckId = o.CheckId
        //                                       AND Result = 0)) AS CheckpointsFailed
        //              ) AS CheckpointsFailed

        //            DROP TABLE #LatestRuns
        //            DROP TABLE #PageRunsResults";
        #endregion

        #region "Checkpoint Performance History Query"
        //public static readonly string checkpointPerformanceHistoryText1 = @"
        //        ;WITH ScanRows
        //        AS
        //        (
        //         SELECT * FROM
        //         (
        //          SELECT ScanId, Finished, RunId, ROW_NUMBER() OVER (PARTITION BY ScanId ORDER BY RunId DESC) AS ScanRowIndex
        //          FROM reporting.Runs r WITH (NOLOCK)
        //          WHERE r.Finished BETWEEN @StartDate AND @FinishDate
        //            AND r.ScanGroupId = @ScanGroupId
        //            AND Status = 2
        //            AND ScanId > 0
        //         ) AS ScanRows
        //         WHERE ScanRowIndex <= 12
        //        )
        //        SELECT * INTO #ScanRows FROM ScanRows

        //        ;WITH PageRunsResults(PageId, RunId, [Url], [Started], [Finished], ScanGroupId, CheckId, Result)
        //        AS
        //        (
        //            SELECT p.PageId,
        //                    r.RunId,
        //                    p.[Url],
        //                    r.[Started],
        //                    r.[Finished],
        //                    r.ScanGroupId,
        //                    res.CheckId,
        //                    res.Result
        //            FROM Pages p WITH (NOLOCK)
        //                INNER JOIN Runs r WITH (NOLOCK)
        //                ON p.RunId = r.RunId
        //                INNER JOIN Results res WITH (NOLOCK)
        //                ON res.PageId = p.PageId
        //                AND res.RunId = r.RunId
        //            WHERE r.RunId IN (Select RunId FROM #ScanRows)
        //             AND IsPage = 1
        //        )
        //        SELECT * INTO #PageRunsResults FROM PageRunsResults

        //        SELECT MAX(Finished) As MaxFinishDate,
        //               ScanRowIndex,
        //            (
        //           SELECT COUNT(1)
        //           FROM
        //           (SELECT DISTINCT CheckId
        //            FROM #PageRunsResults o
        //            WHERE o.RunId IN (SELECT RunId FROM #ScanRows Where ScanRowIndex = oo.ScanRowIndex)) As PagesScanned
        //            ) AS TotalCheckpointsScanned,
        //            ( 
        //           SELECT COUNT(1)
        //                    FROM
        //                    (Select DISTINCT CheckId FROM #PageRunsResults o
        //                  WHERE o.RunId IN (SELECT RunId FROM #ScanRows Where ScanRowIndex = oo.ScanRowIndex)
        //            AND NOT EXISTS(SELECT 1 FROM #PageRunsResults i
        //                      WHERE i.CheckId = o.CheckId
        //                                        AND Result = 0)) As CheckpointsPassed
        //             ) As CheckpointsPassed,
        //             (   
        //           SELECT COUNT(1)
        //                    FROM
        //                    (Select DISTINCT CheckId FROM #PageRunsResults o
        //                  WHERE o.RunId IN (SELECT RunId FROM #ScanRows Where ScanRowIndex = oo.ScanRowIndex)
        //            AND EXISTS(SELECT 1 FROM #PageRunsResults i
        //                      WHERE i.CheckId = o.CheckId
        //                                        AND Result = 0)) As CheckpointsFailed
        //               ) As CheckpointsFailed
        //        FROM #ScanRows oo
        //        GROUP BY ScanRowIndex

        //        DROP TABLE #ScanRows
        //        DROP TABLE #PageRunsResults";
        #endregion

        #region "Page Performance History Query"
        //static readonly string pagePerformanceHistoryText = @"
        //        ;WITH ScanRows
        //        AS
        //        (
        //            SELECT* FROM

        //            (
        //                SELECT ScanId, Finished, RunId, ROW_NUMBER() OVER (PARTITION BY ScanId ORDER BY RunId DESC) AS ScanRowIndex

        //                FROM reporting.Runs r WITH (NOLOCK)
        //                WHERE r.Finished BETWEEN @StartDate AND @FinishDate
        //                    AND r.ScanGroupId = @ScanGroupId

        //                    AND Status = 2

        //                    AND ScanId > 0
        //         ) AS ScanRows

        //            WHERE ScanRowIndex <= 12
        //        )
        //        SELECT* INTO #ScanRows FROM ScanRows

        //        ; WITH PageRunsResults(PageId, RunId, [Url], [Started], [Finished], ScanGroupId, CheckId, Result)
        //        AS
        //        (
        //            SELECT p.PageId,
        //                    r.RunId,
        //                    p.[Url],
        //                    r.[Started],
        //                    r.[Finished],
        //                    r.ScanGroupId,
        //                    res.CheckId,
        //                    res.Result
        //            FROM Pages p WITH (NOLOCK)
        //                INNER JOIN Runs r WITH (NOLOCK)
        //                ON p.RunId = r.RunId
        //                INNER JOIN Results res WITH (NOLOCK)
        //                ON res.PageId = p.PageId
        //                AND res.RunId = r.RunId
        //            WHERE r.RunId IN (Select RunId FROM #ScanRows)
        //             AND IsPage = 1
        //        )
        //        SELECT* INTO #PageRunsResults FROM PageRunsResults

        //        SELECT MAX(Finished) As FinishDate,
        //                ScanRowIndex,
        //             (
        //                    SELECT COUNT(1)
        //           FROM
        //                    (SELECT DISTINCT URL
        //                        FROM #PageRunsResults o
        //            WHERE o.RunId IN (SELECT RunId FROM #ScanRows Where ScanRowIndex = oo.ScanRowIndex)) As PagesScanned
        //             ) AS TotalPagesScanned,
        //             (
        //                 SELECT COUNT(1)
        //                    FROM
        //                    (SELECT DISTINCT URL

        //                        FROM #PageRunsResults o
        //                  WHERE o.RunId IN(SELECT RunId FROM #ScanRows Where ScanRowIndex = oo.ScanRowIndex)
        //            AND NOT EXISTS(SELECT 1 FROM #PageRunsResults i
        //                      WHERE i.Url = o.Url
        //                                        AND Result = 0)) As PassedScans
        //                ) As PassedScans,
        //                (
        //                    SELECT COUNT(1)
        //                    FROM
        //                    (SELECT DISTINCT URL
        //                        FROM #PageRunsResults o
        //                  WHERE o.RunId IN(SELECT RunId FROM #ScanRows Where ScanRowIndex = oo.ScanRowIndex)
        //            AND EXISTS(SELECT 1 FROM #PageRunsResults i
        //                      WHERE i.Url = o.Url
        //                                        AND Result = 0)) As FailedScans
        //                ) As FailedScans
        //        FROM #ScanRows oo
        //        GROUP BY ScanRowIndex

        //        DROP TABLE #ScanRows
        //        DROP TABLE #PageRunsResults";
        #endregion
        #endregion

        #endregion

        public ScanGroupScanAccessor(IConnectionManager connection, ILogger<ScanGroupScanAccessor> logger)
        {
            this.connection = connection;
            _logger = logger;
        }

        public async Task<IEnumerable<ScanPerformance>> GetScanPerformanceByScanGroup(int? scanGroupId, int? userGroupId, CancellationToken cancellationToken, bool scheduledScan = true)
        {
            CommandBuilder commandBuilder = new CommandBuilder(performanceByScan,
                new Dictionary<string, Action<DbParameter>>
                      {
                            { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                            { "@ScheduledScan", p => p.DbType = System.Data.DbType.Boolean },
                            { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                            { "@PermissionType", p => p.DbType = System.Data.DbType.String }
                      },
                      System.Data.CommandType.Text
                  );

            var scansByPerformance = new List<ScanPerformance>();

            using (var command = await commandBuilder.BuildFrom(connection,
                       new Dictionary<string, object>
                       {
                        { "@ScanGroupId", scanGroupId },
                        { "@ScheduledScan", scheduledScan },
                        { "@UserGroupId", userGroupId },
                        { "@PermissionType", "Scan" }
                       }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var scanPerformance = new ScanPerformance
                        {
                            ScanId = Convert.ToInt32(reader["ScanId"].ToString()),
                            ScanName = reader["DisplayName"].ToString(),
                            ScannedPages = Convert.ToInt32(reader["PagesScanned"].ToString()),
                            ScannedCheckpoints = Convert.ToInt32(reader["ScannedCheckpoints"].ToString()),
                            CheckpointSuccess = Convert.ToInt32(reader["PassedCheckpoints"].ToString()),
                            CheckpointFailure = Convert.ToInt32(reader["FailedCheckpoints"].ToString()),
                            CheckpointFailurePercent = Convert.ToDouble(reader["CheckpointFailurePercent"].ToString())
                        };

                        scansByPerformance.Add(scanPerformance);
                    }
                }
            }

            return scansByPerformance;
        }

        public async Task<IEnumerable<ScanGroupHistory>> GetScanGroupHistory(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true)
        {
            CommandBuilder commandBuilder = new CommandBuilder(scangroupHistoryText,
                new Dictionary<string, Action<DbParameter>>
                      {
                            { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                            { "@ScheduledScan", p => p.DbType = System.Data.DbType.Boolean }
                      },
                      System.Data.CommandType.Text
                  );

            var scanGroupHistories = new List<ScanGroupHistory>();

            using (var command = await commandBuilder.BuildFrom(connection,
                      new Dictionary<string, object>
                      {
                        { "@ScanGroupId", scanGroupId },
                        { "@ScheduledScan", scheduledScan }
                      }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {                  
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var scanGroupHistory = new ScanGroupHistory
                        {
                            ScanGroupRunId = Convert.ToInt32(reader["ScanGroupRunId"].ToString()),
                            ScanGroupId = Convert.ToInt32(reader["ScanGroupId"].ToString()),
                            RunDate = Convert.ToDateTime(reader["RunDate"].ToString()),
                            TotalPages = Convert.ToInt32(reader["TotalPages"].ToString()),
                            PassedPages = Convert.ToInt32(reader["PassedPages"].ToString()),
                            FailedPages = Convert.ToInt32(reader["FailedPages"].ToString()),
                            PassedPagePercent = Convert.ToDecimal(reader["PassedPagesPercent"].ToString()),
                            FailedPagePercent = Convert.ToDecimal(reader["FailedPagesPercent"].ToString()),
                            TotalCheckpoints = Convert.ToInt32(reader["TotalCheckpoints"].ToString()),
                            PassedCheckpoints = Convert.ToInt32(reader["PassedCheckpoints"].ToString()),
                            FailedCheckpoints = Convert.ToInt32(reader["FailedCheckpoints"].ToString()),
                            PassedCheckpointPercent = Convert.ToDecimal(reader["PassedCheckpointsPercent"].ToString()),
                            FailedCheckpointPercent = Convert.ToDecimal(reader["FailedCheckpointsPercent"].ToString())
                        };

                        scanGroupHistories.Add(scanGroupHistory);
                    }
                }
            }

            return scanGroupHistories;
        }

        public async Task<IEnumerable<PageFailure>> GetTop10PageFailures(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true)
        {
            CommandBuilder commandBuilder = new CommandBuilder(top10PageFailuresText,
                  new Dictionary<string, Action<DbParameter>>
                  {
                        { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@ScheduledScan", p => p.DbType = System.Data.DbType.Boolean }
                  },
                  System.Data.CommandType.Text
              );

            var pageFailures = new List<PageFailure>();

            using (var command = await commandBuilder.BuildFrom(connection,
                     new Dictionary<string, object>
                     {
                        { "@ScanGroupId", scanGroupId },
                        { "@ScheduledScan", scheduledScan }
                     }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {                   
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var pageFailure = new PageFailure
                        {
                            CurrentCheckpointFailures = Convert.ToInt32(reader["CurrentCheckpointFailures"].ToString()),
                            OneRunBackCheckpointFailures = Convert.ToInt32(reader["OneRunBackCheckpointFailures"].ToString()),
                            TwoRunsBackCheckpointFailures = Convert.ToInt32(reader["TwoRunsBackCheckpointFailures"].ToString()),                            
                            PageUrl = reader["URL"].ToString(),
                            Priority1Failures = Convert.ToInt32(reader["Priority1Failures"].ToString()),
                            Priority2Failures = Convert.ToInt32(reader["Priority2Failures"].ToString()),
                            Priority3Failures = Convert.ToInt32(reader["Priority3Failures"].ToString())
                        };

                        pageFailures.Add(pageFailure);
                    }
                }
            }

            return pageFailures;
        }

        public async Task<ScanGroupPerformanceMetrics> GetPagePerformanceMetrics(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true)
        {
            CommandBuilder commandBuilder = new CommandBuilder(scanGroupPagePerformanceText,
                 new Dictionary<string, Action<DbParameter>>
                 {
                        { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@ScheduledScan", p => p.DbType = System.Data.DbType.Boolean }
                 },
                 System.Data.CommandType.Text
             );

            var scanGroupPerformanceMetrics = new ScanGroupPerformanceMetrics
            {
                ScanGroupId = 0,
                PerformanceType = "Page",
                Metrics = new PerformanceMetrics()
            };

            using (var command = await commandBuilder.BuildFrom(connection,
                    new Dictionary<string, object>
                    {
                        { "@ScanGroupId", scanGroupId },
                        { "@ScheduledScan", scheduledScan }
                    }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scanGroupPerformanceMetrics.ScanGroupId = reader.IsDBNull(1) == true ? 0 : reader.GetInt32(1);

                        var scanTotal = Convert.ToInt32(reader["TotalPages"].ToString());
                        var scanPasses = Convert.ToInt32(reader["PassedPages"].ToString());
                        var scanFailures = Convert.ToInt32(reader["FailedPages"].ToString());
                        var scanPassPercentage = Convert.ToDouble(reader["PassedPagesPercent"].ToString());
                        var scanFailPercentage = Convert.ToDouble(reader["FailedPagesPercent"].ToString());

                        ScanCalculationResult percentPageResults = CalculateScanPercentages(scanTotal, scanFailures);

                        var pagePerformanceMetric = new PerformanceMetrics
                        {
                            ScanTotal = scanTotal,
                            PassedTotal = scanPasses,
                            PassedPercent = scanPassPercentage,
                            FailedTotal = scanFailures,
                            FailedPercent = scanFailPercentage
                        };

                        scanGroupPerformanceMetrics.Metrics = pagePerformanceMetric;
                    }
                }
            }

            return scanGroupPerformanceMetrics;
        }

        public async Task<ScanGroupPerformanceMetrics> GetCheckPointPerformanceMetrics(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true)
        {
            CommandBuilder commandBuilder = new CommandBuilder(scanGroupCheckPointPerformanceText,
                 new Dictionary<string, Action<DbParameter>>
                 {
                        { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@ScheduledScan", p => p.DbType = System.Data.DbType.Boolean }
                 },
                 System.Data.CommandType.Text
             );

            var scanGroupCheckPointPerformanceMetrics = new ScanGroupPerformanceMetrics
            {
                ScanGroupId = 0,
                PerformanceType = "Checkpoint",
                Metrics = new PerformanceMetrics()
            };

            using (var command = await commandBuilder.BuildFrom(connection,
                    new Dictionary<string, object>
                    {
                        { "@ScanGroupId", scanGroupId },
                        { "@ScheduledScan", scheduledScan }
                    }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scanGroupCheckPointPerformanceMetrics.ScanGroupId = reader.IsDBNull(1) == true ? 0 : reader.GetInt32(1);

                        var checkpointTotal = Convert.ToInt32(reader["TotalCheckpoints"].ToString());
                        var checkpointPasses = Convert.ToInt32(reader["PassedCheckpoints"].ToString());
                        var checkpointFailures = Convert.ToInt32(reader["FailedCheckpoints"].ToString());
                        var passedCheckpointPercent = Convert.ToDouble(reader["PassedCheckpointsPercent"].ToString());
                        var failedCheckpointPercent = Convert.ToDouble(reader["FailedCheckpointsPercent"].ToString());

                        ScanCalculationResult percentCheckpointResults = CalculateScanPercentages(checkpointTotal, checkpointFailures);

                        var checkPointPerformanceMetric = new PerformanceMetrics
                        {
                            ScanTotal = checkpointTotal,
                            PassedTotal = checkpointPasses,
                            PassedPercent = passedCheckpointPercent,
                            FailedTotal = checkpointFailures,
                            FailedPercent = failedCheckpointPercent
                        };

                        scanGroupCheckPointPerformanceMetrics.Metrics = checkPointPerformanceMetric;
                    }
                }
            }

            return scanGroupCheckPointPerformanceMetrics;
        }

        private ScanCalculationResult CalculateScanPercentages(int totals, int failed)
        {
            double successPercentage = 0.0;
            double failedPercentage = 0.0;

            if (totals > 0)
            {
                var difference = Math.Abs(totals - failed);
                successPercentage = ((double)difference / totals) * 100;
                failedPercentage = ((double)failed / totals) * 100;
            }
 
            return new ScanCalculationResult
            {
                Successful = Math.Abs(Math.Round(successPercentage, 1)),
                Failed = Math.Abs(Math.Round(failedPercentage, 1))
            };
        }

        private struct ScanCalculationResult
        {
            public double Successful;
            public double Failed;
        }
    }
}
