using ComplianceSheriff.Scans;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ComplianceSheriff.ScanGroups;

namespace ComplianceSheriff.AdoNet.ScanGroups
{
    class ScanGroupAccessor : IScanGroupAccessor
    {

        #region "SQL Queries"

        static readonly string getScanGroupById = @"SELECT * FROM ScanGroups WHERE ScanGroupId = @ScanGroupId";

        static readonly string getScanGroupByName = @"SELECT * FROM ScanGroups WHERE DisplayName = @DisplayName";

        static readonly string getAllScanGroups = @"SELECT sg.ScanGroupId, DisplayName FROM ScanGroups sg
                                                        INNER JOIN dbo.udfScanGroupsByPermission(@UserGroupId, @PermissionType) sgbp
                                                         ON sgbp.ScanGroupId = sg.ScanGroupId
                                                        ORDER BY DisplayName";

        static readonly string scheduledScangrouplistText = @"
                 SELECT * FROM
                 (
                     Select sg.ScanGroupId, 
		                    DisplayName,
		                    (SELECT TOP 1 RunDate FROM reporting.ScanGroupRuns 
                                Where ScanGroupId = sg.ScanGroupId 
                                  AND ScheduledScan = @ScheduledScan
                                ORDER BY RunDate DESC) As LastScanDate
                     FROM ScanGroups sg
                          INNER JOIN dbo.udfScanGroupsByPermission(@UserGroupId, @PermissionType) sgbp
                            ON sgbp.ScanGroupId = sg.ScanGroupId
                ) As ScheduledScanGroups
                Where LastScanDate IS NOT NULL
                ORDER BY DisplayName";

        static readonly string getSubGroupsAndScansByScanGroupId = @"
              SELECT sgsg.SubgroupId, DisplayName
              FROM ScanGroups sg
                INNER JOIN ScanGroupSubgroups sgsg
	              ON sgsg.SubgroupId = sg.ScanGroupId
                INNER JOIN [dbo].[udfScanGroupsByPermission] (@UserGroupId, 'ScanGroup') sgp
	              ON sgp.ScanGroupId = sgsg.ScanGroupId
              WHERE sgsg.ScanGroupId = @ScanGroupId
              ORDER BY DisplayName

              SELECT s.ScanId, DisplayName
              FROM Scans s
              INNER JOIN ScanGroupScans sgs
	            ON sgs.ScanId = s.ScanId
	          INNER JOIN [dbo].[udfScansByPermission](@UserGroupId, 'Scan') sbp
	            ON sbp.ScanId = s.ScanId
              WHERE sgs.ScanGroupId = @ScanGroupId
              ORDER BY DisplayName";

        #endregion

        public static readonly CommandBuilder GetScanGroupReportCommand = new CommandBuilder(@"dbo.generateScanGroupReport",
                new Dictionary<string, Action<DbParameter>>
                {
                    { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                    { "@StartDate", p => p.DbType = System.Data.DbType.DateTime },
                    { "@FinishDate", p => p.DbType = System.Data.DbType.DateTime },
                },
                System.Data.CommandType.StoredProcedure
            );

        private readonly IConnectionManager connection;
        private readonly ILogger<ScanGroupAccessor> _logger;

        public ScanGroupAccessor(IConnectionManager connection, ILogger<ScanGroupAccessor> logger)
        {
            this.connection = connection;
            this._logger = logger;
        }

        public async Task<IEnumerable<ScheduledScanGroup>> GetScheduledScanGroups(int userGroupId, CancellationToken cancellationToken, bool scheduledScan = true)
        {
            try
            {
                CommandBuilder commandBuilder = new CommandBuilder(scheduledScangrouplistText,
                     new Dictionary<string, Action<DbParameter>>
                           {
                             { "@ScheduledScan", p => p.DbType = System.Data.DbType.Boolean },
                             { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                             { "@PermissionType", p => p.DbType = System.Data.DbType.String }
                           },
                           System.Data.CommandType.Text
                       );

                using (var command = await commandBuilder.BuildFrom(connection,
                       new Dictionary<string, object>
                       {
                            { "@ScheduledScan", scheduledScan },
                            { "@UserGroupId", userGroupId },
                            { "@PermissionType", "ScanGroup" }
                       }, cancellationToken))
                {
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        var scanGroupList = new List<ScheduledScanGroup>();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            scanGroupList.Add(new ScheduledScanGroup
                            {
                                ScanGroupId = Convert.ToInt32(reader["ScanGroupId"]),
                                DisplayName = reader["DisplayName"].ToString(),
                                LastScanDate = !string.IsNullOrWhiteSpace(reader["LastScanDate"].ToString()) ? Convert.ToDateTime(reader["LastScanDate"].ToString()) : (DateTime?)null
                            });
                        }

                        return scanGroupList;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        public async Task<IEnumerable<ScanGroupReport>> GetScanGroupReport(int scanGroupId, DateRange range, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting Execution of Scangroup Report Procedure");

                using (var command = await GetScanGroupReportCommand.BuildFrom(connection, new Dictionary<string, object>
                {
                    { "@ScanGroupId", scanGroupId },
                    { "@StartDate", range.StartDate.Value },
                    { "@FinishDate", range.EndDate.Value },
                }, cancellationToken))

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    var scangroupResponses = new List<ScanGroupReport>();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scangroupResponses.Add(new ScanGroupReport
                        {
                            ReportType = reader["ReportType"].ToString(),
                            ReportTargetId = Convert.ToInt32(reader["ReportTargetId"]),
                            ParentScanGroupId = reader["ParentScanGroupId"] is DBNull ? (int?)null : Convert.ToInt32(reader["ParentScanGroupId"]),
                            GroupName = reader["Group Name"].ToString(),
                            ScanName = reader["Scan Name"].ToString(),
                            MinRunFinished = Convert.ToDateTime(reader["MinRunFinished"]),
                            MaxRunFinished = Convert.ToDateTime(reader["MaxRunFinished"]),
                            TotalAnalyzedOccurrences = Convert.ToUInt64(reader["Total Analyzed Occurrences"]),

                            FailedOccurrences = Convert.ToUInt64(reader["Failed Occurrences"]),
                            Failed_P1_Occurrences = Convert.ToUInt32(reader["Failed P1 Occurrences"]),
                            Failed_P2_Occurrences = Convert.ToUInt32(reader["Failed P2 Occurrences"]),
                            Failed_P3_Occurrences = Convert.ToUInt32(reader["Failed P3 Occurrences"]),

                            WarningOccurrences = Convert.ToUInt64(reader["Warning Occurrences"]),
                            Warnings_P1_Occurrences = Convert.ToUInt32(reader["Warnings P1 Occurrences"]),
                            Warnings_P2_Occurrences = Convert.ToUInt32(reader["Warnings P2 Occurrences"]),
                            Warnings_P3_Occurrences = Convert.ToUInt32(reader["Warnings P3 Occurrences"]),

                            Failed_P1_or_P2_Occurrences = Convert.ToUInt32(reader["Failed P1 or P2 Occurrences"]),
                            Failed_or_Warning_Occurrences = Convert.ToUInt32(reader["Failed or Warning Occurrences"]),
                            Failed_or_Warning_P1_Occurrences = Convert.ToUInt32(reader["Failed or Warning P1 Occurrences"]),
                            Failed_or_Warning_P1_or_P2_Occurrences = Convert.ToUInt32(reader["Failed Or Warning P1 or P2 Occurrences"]),
                            Occurrences_No_Failures_Or_Warnings = Convert.ToUInt32(reader["Occurrences with No Failures Or Warnings"]),
                            Passing_P1_Occurrences = Convert.ToUInt32(reader["Passing P1 Occurrences"]),
                            Passing_P1_or_P2_Occurrences = Convert.ToUInt32(reader["Passing P1 or P2 Occurrences"]),

                            All_Passing_Occurrences = Convert.ToUInt32(reader["All Passing Occurrences"]),

                            Occurrences_needing_Visual_Inspection = Convert.ToUInt32(reader["Occurrences needing Visual Inspection"]),
                            Visual_Inspection_P1_Occurrences = Convert.ToUInt32(reader["Visual Inspection P1 Occurrences"]),
                            Visual_Inspection_P2_Occurrences = Convert.ToUInt32(reader["Visual Inspection P2 Occurrences"]),
                            Visual_Inspection_P3_Occurrences = Convert.ToUInt32(reader["Visual Inspection P3 Occurrences"]),
                            Occurrences_needing_Visual_Inspection_on_P1s_or_P2s = Convert.ToUInt32(reader["Occurrences needing Visual Inspection on P1s or P2s"]),

                            Checkpoints = Convert.ToUInt32(reader["Checkpoints"]),
                            P1_Checkpoints = Convert.ToUInt32(reader["# of P1 Checkpoints"]),
                            P2_Checkpoints = Convert.ToUInt32(reader["# of P2 Checkpoints"]),
                            P3_Checkpoints = Convert.ToUInt32(reader["# of P3 Checkpoints"]),
                            FailedCheckPoints = Convert.ToUInt32(reader["Failed CheckPoints"]),
                            Failed_P1_Checkpoints = Convert.ToUInt32(reader["Failed P1 Checkpoints"]),
                            Failed_P2_Checkpoints = Convert.ToUInt32(reader["Failed P2 Checkpoints"]),
                            Failed_P3_Checkpoints = Convert.ToUInt32(reader["Failed P3 Checkpoints"]),
                            Failed_P1_or_P2_Checkpoints = Convert.ToUInt32(reader["Failed P1 or P2 Checkpoints"]),

                            Warning_CheckPoints = Convert.ToUInt32(reader["Warning Checkpoints"]),
                            Warning_P1_CheckPoints = Convert.ToUInt32(reader["Warning P1 Checkpoints"]),
                            Warning_P2_CheckPoints = Convert.ToUInt32(reader["Warning P2 Checkpoints"]),
                            Warning_P3_CheckPoints = Convert.ToUInt32(reader["Warning P3 Checkpoints"]),

                            Failed_or_Warning_Checkpoints = Convert.ToUInt32(reader["Failed or Warning Checkpoints"]),
                            Failed_or_Warning_P1_Checkpoints = Convert.ToUInt32(reader["Failed or Warning P1 Checkpoints"]),
                            Failed_or_Warning_P1_or_P2_Checkpoints = Convert.ToUInt32(reader["Failed or Warning P1 or P2 Checkpoints"]),

                            CheckPoints_needing_Visual_Inspection = Convert.ToUInt32(reader["CheckPoints needing Visual Inspection"]),
                            Visual_Inspection_P1_CheckPoints = Convert.ToUInt32(reader["Visual Inspection P1 CheckPoints"]),
                            Visual_Inspection_P2_CheckPoints = Convert.ToUInt32(reader["Visual Inspection P2 CheckPoints"]),
                            Visual_Inspection_P3_CheckPoints = Convert.ToUInt32(reader["Visual Inspection P3 CheckPoints"]),

                            Pages = Convert.ToUInt32(reader["Pages"]),

                            Pages_with_Failures = Convert.ToUInt32(reader["Pages with Failures"]),
                            Pages_P1_Failures = Convert.ToUInt32(reader["Pages with P1 Failures"]),
                            Pages_P2_Failures = Convert.ToUInt32(reader["Pages with P2 Failures"]),
                            Pages_P3_Failures = Convert.ToUInt32(reader["Pages with P3 Failures"]),

                            Pages_with_Warnings = Convert.ToUInt32(reader["Pages with Warnings"]),
                            Pages_P1_Warnings = Convert.ToUInt32(reader["Pages with P1 Warnings"]),
                            Pages_P2_Warnings = Convert.ToUInt32(reader["Pages with P2 Warnings"]),
                            Pages_P3_Warnings = Convert.ToUInt32(reader["Pages with P3 Warnings"]),

                            Pages_needing_Visual_Inspection = Convert.ToUInt32(reader["Pages needing Visual Inspection"]),
                            Pages_with_P1_Visual_Inspections = Convert.ToUInt32(reader["Pages with P1 Visual Inspections"]),
                            Pages_with_P2_Visual_Inspections = Convert.ToUInt32(reader["Pages with P2 Visual Inspections"]),
                            Pages_with_P3_Visual_Inspections = Convert.ToUInt32(reader["Pages with P3 Visual Inspections"]),

                            Pages_Failures_or_Warnings = Convert.ToUInt32(reader["Pages with Failures or Warnings"]),

                            Pages_P1_Failures_or_Warnings = Convert.ToUInt32(reader["Pages with P1 Failures or Warnings"]),
                            Pages_P1_or_P2_Failures_or_Warnings = Convert.ToUInt32(reader["Pages with P1 or P2 Failures or Warnings"]),

                            Pages_needing_Visual_Inspection_on_P1s_or_P2s = Convert.ToUInt32(reader["Pages needing Visual Inspection on P1s or P2s"]),


                        });
                    }

                    _logger.LogInformation("Execution of Scangroup Report Procedure Completed");

                    return scangroupResponses;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading ScanGroup Report");
                throw;
            }

        }

        public async Task<IEnumerable<ScanGroupListItem>> GetAllScanGroupListByPermission(int userGroupId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(getAllScanGroups,
                new Dictionary<string, Action<DbParameter>>
                      {
                         { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                         { "@PermissionType", p => p.DbType = System.Data.DbType.String }
                      },
                      System.Data.CommandType.Text
                  );

            var scanGroupList = new List<ScanGroupListItem>();

            using (var command = await commandBuilder.BuildFrom(connection,
                            new Dictionary<string, object>
                            {
                                { "@UserGroupId", userGroupId },
                                { "@PermissionType", "ScanGroup" }
                            }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scanGroupList.Add(new ScanGroupListItem
                        {
                            ScanGroupId = Convert.ToInt32(reader["ScanGroupId"]),
                            DisplayName = reader["DisplayName"].ToString()
                        });
                    }
                }
            }

            return scanGroupList;
        }

        public async Task<SubGroupScansResponse> GetSubGroupsAndScansByScanGroupId(int userGroupId, int scanGroupId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(getSubGroupsAndScansByScanGroupId,
                new Dictionary<string, Action<DbParameter>>
                      {
                         { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                         { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                         { "@PermissionType", p => p.DbType = System.Data.DbType.String }
                      },
                      System.Data.CommandType.Text
                  );

            var subGroupScansResponse = new SubGroupScansResponse();

            using (var command = await commandBuilder.BuildFrom(connection,
                            new Dictionary<string, object>
                            {
                                { "@ScanGroupId", scanGroupId },
                                { "@UserGroupId", userGroupId },
                                { "@PermissionType", "ScanGroup" }
                            }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        subGroupScansResponse.SubGroups.Add(new ScanGroupListItem
                        {
                            ScanGroupId = Convert.ToInt32(reader["SubGroupId"].ToString()),
                            DisplayName = reader["DisplayName"].ToString()
                        });
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        subGroupScansResponse.Scans.Add(new ScanListItem
                        {
                            ScanId = Convert.ToInt32(reader["ScanId"].ToString()),
                            DisplayName = reader["DisplayName"].ToString()
                        });
                    }
                }
            }

            return subGroupScansResponse;
        }

        public async Task<IEnumerable<ScanGroup>> GetAllScanGroups(CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(getAllScanGroups,
                new Dictionary<string, Action<DbParameter>>
                { },
                      System.Data.CommandType.Text
                  );

            var scanGroupList = new List<ScanGroup>();

            using (var command = await commandBuilder.BuildFrom(connection,
                            new Dictionary<string, object>
                            { }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scanGroupList.Add(new ScanGroup
                        {
                            ScanGroupId = Convert.ToInt32(reader["ScanGroupId"]),
                            DisplayName = reader["DisplayName"].ToString(),
                            //LastScanDate = !string.IsNullOrWhiteSpace(reader["LastScanDate"].ToString()) ? Convert.ToDateTime(reader["LastScanDate"].ToString()) : (DateTime?)null
                        });
                    }
                }
            }

            return scanGroupList;
        }

        public async Task<ScanGroup> GetScanGroupByDisplayName(string displayName, CancellationToken cancellationToken)
        {
            try
            {
                CommandBuilder commandBuilder = new CommandBuilder(getScanGroupByName,
                     new Dictionary<string, Action<DbParameter>>
                           {
                             { "@DisplayName", p => p.DbType = System.Data.DbType.String }
                           },
                           System.Data.CommandType.Text
                       );

                using (var command = await commandBuilder.BuildFrom(connection,
                       new Dictionary<string, object>
                       {
                            { "@DisplayName", displayName }
                       }, cancellationToken))
                {
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        ScanGroup scanGroup = null;

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            scanGroup = new ScanGroup
                            {
                                ScanGroupId = Convert.ToInt32(reader["ScanGroupId"]),
                                DisplayName = reader["DisplayName"].ToString(),
                            };
                        }

                        return scanGroup;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }

        public async Task<ScanGroup> GetScanGroupByScanGroupId(int scanGroupId, CancellationToken cancellationToken)
        {
            try
            {
                CommandBuilder commandBuilder = new CommandBuilder(getScanGroupById,
                     new Dictionary<string, Action<DbParameter>>
                           {
                             { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 }
                           },
                           System.Data.CommandType.Text
                       );

                using (var command = await commandBuilder.BuildFrom(connection,
                       new Dictionary<string, object>
                       {
                            { "@ScanGroupId", scanGroupId }
                       }, cancellationToken))
                {
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        ScanGroup scanGroup = null;

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            scanGroup = new ScanGroup
                            {
                                ScanGroupId = Convert.ToInt32(reader["ScanGroupId"]),
                                DisplayName = reader["DisplayName"].ToString(),
                            };
                        }

                        return scanGroup;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }
    }
}
