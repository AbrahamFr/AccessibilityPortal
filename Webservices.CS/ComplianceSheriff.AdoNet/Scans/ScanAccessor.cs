using ComplianceSheriff.Scans;
using ComplianceSheriff.FileSystem;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using ComplianceSheriff.CheckpointGroups;
using System.Text.RegularExpressions;
using ComplianceSheriff.Extensions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using ComplianceSheriff.DataFormatter;
using ComplianceSheriff.TextFormatter;

namespace ComplianceSheriff.AdoNet.Scans
{
    public class ScanAccessor : IScanAccessor
    {
        private readonly IConnectionManager _connection;
        private readonly ILogger<ScanAccessor> _logger;
        private readonly IFileSystemService _fileSystemService;
        private readonly IDataFormatterService _dataFormatterService;
        private readonly ICheckpointGroupsAccessor _checkpointGroupAccessor;

        public ScanAccessor(IConnectionManager connection, ILogger<ScanAccessor> logger)
        {
            this._connection = connection;
            this._logger = logger;
        }

        #region "SQL Queries"

        #region "CheckScanExists"
            public static readonly string sqlCheckScanExists = @"
                SELECT CASE WHEN EXISTS (SELECT TOP 1 *
                                         FROM Scans 
                                         WHERE ScanId = @ScanId) 
                            THEN CAST (1 AS BIT) 
                            ELSE CAST (0 AS BIT) END AS ScanExists";
        #endregion

        #region "GetAllScansList"

        static readonly string sqlGetAllScansList = @"
                Select s.ScanId,
                        s.IsMonitor,
                        s.BaseUrl,
                        s.DisplayName,
                        s.IncludedDomains,
                        s.IncludeFilter,
                        s.ExcludeFilter,
                        s.UserAgent,
                        s.IncludeMSOfficeDocs,
                        s.IncludeOtherDocs,
                        s.RetestAllPages,
                        s.AlertMode,
                        s.AlertDelay,
                        s.AlertSendTo,
                        s.AlertSubject,
                        s.StartPages,
                        s.DateCreated,
                        s.DateModified,
                        s.LocalClientId,                 
	                    sbp.ScanPermission
                FROM Scans s
                INNER JOIN dbo.udfGetScanListPermissionByUserGroupId(@UserGroupId, @PermissionType) sbp
                ON s.ScanId = sbp.ScanId
                order by s.DisplayName";
        #endregion

        #region "GetRecentScans"

        static readonly string sqlGetRecentScans = @"
            
            DECLARE @SortColumnAndDirection varchar(2000), @OffsetRowCount int, @TotalScanRecords int
			DECLARE @sql nvarchar(max), @PagingSQL varchar(4000), @AllowPaging bit

            --DEFAULT FOR PAGING IS TRUE
            SET @AllowPaging = 1

            IF @SortDirection IS NULL OR @SortDirection = ''
                SET @SortDirection = 'ASC'

            -- DEFAULTS TO Sorting By Severity Importance (High, Med, Low) then Impact
            -- We included Issue in all sorts to keep rows consistent when returned
            IF @SortColumn IS NULL OR @SortColumn = '' 
                BEGIN
                    SET @SortColumnAndDirection = 'ScanName ' + @SortDirection
                END
            ELSE
                BEGIN
                    SET @SortColumnAndDirection = '[' + @SortColumn + '] ' + @SortDirection
                END

            IF @CurrentPage IS NULL OR @RowsToFetch IS NULL
                BEGIN
                    SET @AllowPaging = 0
                END
                       
            IF @AllowPaging = 1
                BEGIN
			        IF @CurrentPage = 1
			            BEGIN
					    SET @OffsetRowCount = 0
				        END
				    ELSE
				        BEGIN
					    SET @OffsetRowCount = (@CurrentPage-1) * @RowsToFetch
				        END
                END

			--Paging
            IF @AllowPaging = 1
                BEGIN
			        SET @PagingSQL = ' OFFSET ' + CAST(@OffsetRowCount AS nvarchar(50)) + ' ROWS FETCH NEXT ' +  CAST(@RowsToFetch AS nvarchar(50)) + ' ROWS ONLY '
                END
			
			DECLARE @CheckpointGroups TABLE(
	            [Priority] int,
	            CheckpointGroupId nvarchar(32)
            )

            INSERT INTO @CheckpointGroups VALUES (0, '0200')
            INSERT INTO @CheckpointGroups VALUES (1, 'W21_A')
            INSERT INTO @CheckpointGroups VALUES (2, 'W21_AA')
            INSERT INTO @CheckpointGroups VALUES (3, 'W21_AAA')

            Select  scpg.ScanId, 
		            scpg.CheckpointGroupId, 
		            cpg.Priority
            INTO #checkpointGroups
            FROM ScanCheckpointGroups scpg
              INNER JOIN @CheckpointGroups cpg
                ON cpg.CheckpointGroupId = scpg.CheckpointGroupId
              INNER JOIN dbo.udfGetScanListPermissionByUserGroupId(@UserGroupId, @PermissionType) sbp
                 ON scpg.ScanId = sbp.ScanId

             SELECT * 
             INTO #TempRunResults
             FROM
              (
              SELECT s.ScanId,
		            RunId,
		            ROW_NUMBER() OVER (
		              PARTITION BY s.ScanId
		              ORDER BY RunId DESC) As RowNumber,
		            s.DisplayName As ScanName,
		            r.Health As HealthScore,
		            r.[Status],
		            r.Finished,
		            r.Started,
		            s.BaseUrl,
                    CAST(SUBSTRING(StartPages, CHARINDEX('<Path>',StartPages), (CHARINDEX('</Path>',StartPages) + 7) - CHARINDEX('<Path>',StartPages)) AS xml).value('/Path[1]', 'varchar(200)') As UrlPath,
		            (Select COUNT(1) FROM Pages WHERE RunId = r.RunId AND IsPage = 1) As TotalPagesRan,
                    CAST(SUBSTRING(StartPages, CHARINDEX('<PageLimit>',StartPages), (CHARINDEX('</PageLimit>',StartPages) + 12) - CHARINDEX('<PageLimit>',StartPages)) AS xml).value('/PageLimit[1]', 'int') AS PageLimit,
		            CAST(SUBSTRING(StartPages, CHARINDEX('<Levels>',StartPages), (CHARINDEX('</Levels>',StartPages) + 9) - CHARINDEX('<Levels>',StartPages)) AS xml).value('/Levels[1]', 'int') AS Levels,
                    dbo.fnCalculateMaxPageLevel(RunId, default) AS ScannedLevels
              FROM Scans s WITH (NOLOCK)
              INNER JOIN dbo.udfGetScanListPermissionByUserGroupId(@UserGroupId, @PermissionType) sbp
                 ON s.ScanId = sbp.ScanId
              LEFT OUTER JOIN Runs r WITH (NOLOCK)
                 ON r.ScanId = s.ScanId
              WHERE ([Status] IN (1,2,3,4,8) OR [STATUS] IS NULL)
              ) ScanRuns
              WHERE RowNumber < 3

              SELECT *,
		             ISNULL((SELECT HealthScore FROM #TempRunResults a WHERE a.ScanId = b.ScanId AND RowNumber = 2), 0) AS PreviousRunHealthScore
              INTO #RunResultsPreviousHealthScore
              FROM #TempRunResults b
              WHERE RowNumber = 1

              SET @TotalScanRecords = (SELECT COUNT(1) FROM #RunResultsPreviousHealthScore)
			  
              SET @sql = 'SELECT *,
	                  HealthScore - PreviousRunHealthScore AS HealthScoreChange,
                      CASE WHEN PreviousRunHealthScore > 0 THEN 
		                ROUND(100 * (CAST((ABS(HealthScore - PreviousRunHealthScore)) AS Float) / CAST(HealthScore AS float)), 0)
		              ELSE 0.00
		              END AS HealthScoreChangePercent,
		              (SELECT TOP 1 ShortDescription 
                       FROM #checkpointGroups tmpcpg
                       INNER JOIN CheckpointGroups cpg
                         ON tmpcpg.CheckpointGroupId = cpg.CheckpointGroupId
                       WHERE ScanId = #RunResultsPreviousHealthScore.ScanId 
                       ORDER BY [Priority] DESC) As CheckpointGroupDescription,
		              (SELECT TOP 1 tmpcpg.CheckpointGroupId 
                       FROM #checkpointGroups tmpcpg
                       INNER JOIN CheckpointGroups cpg
                         ON tmpcpg.CheckpointGroupId = cpg.CheckpointGroupId
                       WHERE ScanId = #RunResultsPreviousHealthScore.ScanId 
                       ORDER BY [Priority] DESC) As CheckpointGroupId
              FROM #RunResultsPreviousHealthScore ORDER BY ' + @SortColumnAndDirection + @PagingSQL
              
			  exec(@sql)

              SELECT @TotalScanRecords AS TotalScanRecords

              DROP TABLE #TempRunResults
              DROP TABLE #RunResultsPreviousHealthScore
              DROP TABLE #checkpointGroups";

        #endregion

        #region "GetAllScanIdsByScanGroupID"
        static readonly string sqlGetAllScanIdsByScanGroupId = @"
                        Select ScanId FROM ScanGroupScans
                        WHERE ScanGroupId = @ScanGroupId";
        #endregion

        #region "GetResultViewIdByScanId"
        static readonly string sqlGetResultViewIdByScanId = @"
                        Select ResultViewId FROM ResultViewScans
                        WHERE ScanId = @ScanId";
        #endregion

        #region "GetScanByScanId"
        static readonly string sqlGetScanByScanId = @"
                    Select s.ScanId,
                            s.IsMonitor,
                            s.BaseUrl,
                            s.DisplayName,
                            s.IncludedDomains,
                            s.IncludeFilter,
                            s.ExcludeFilter,
                            s.UserAgent,
                            s.IncludeMSOfficeDocs,
                            s.IncludeOtherDocs,
                            s.RetestAllPages,
                            s.AlertMode,
                            s.AlertDelay,
                            s.AlertSendTo,
                            s.AlertSubject,
                            s.StartPages,
                            s.DateCreated,
                            s.DateModified,
                            s.LocalClientId,                 
	                        sbp.ScanPermission
                    FROM Scans s
                    INNER JOIN dbo.udfGetScanListPermissionByUserGroupId(@UserGroupId, @PermissionType) sbp
                    ON s.ScanId = sbp.ScanId
                    Where s.ScanId = @ScanId";
        #endregion

        #region "GetScanCheckpointGroupByScanId"
        static readonly string sqlGetScanCheckpointGroupByScanId = @"
                    Select cg.CheckpointGroupId
                          ,cg.ShortDescription
                    FROM Scans s INNER JOIN ScanCheckpointGroups scg ON s.ScanId = scg.ScanId
                         INNER JOIN CheckpointGroups cg ON scg.CheckpointGroupId = cg.CheckpointGroupId
                    Where s.ScanId = @ScanId";
        #endregion

        #region "GetScanByScanName"
        static readonly string sqlGetScanDetailsByScanNameText = @"
                Select s.ScanId,
                       s.DisplayName
                from Scans s
                Where s.DisplayName Like @ScanName";
        #endregion

        #region "GetScansByScanIdList"
            static readonly string sqlGetScanIdList = @"
                    SELECT s.ScanId FROM Scans s
                    INNER JOIN dbo.udfGetScanListPermissionByUserGroupId(@UserGroupId, @PermissionType) sbp
                     ON s.ScanId = sbp.ScanId
                    Where s.ScanId IN (SELECT * FROM [dbo].[udf_STRING_SPLIT](@ScanIds, ','))";
        #endregion

        #endregion

        public ScanAccessor(IConnectionManager connection,
                            ILogger<ScanAccessor> logger,
                            IFileSystemService fileSystemService,
                            IDataFormatterService dataFormatterService,
                            ICheckpointGroupsAccessor checkpointGroupsAccessor)
        {
                _connection = connection;
                _logger = logger;
                _fileSystemService = fileSystemService;
                _dataFormatterService = dataFormatterService;
                _checkpointGroupAccessor = checkpointGroupsAccessor;
        }       
        public async Task<List<Scan>> GetScansList(int userGroupId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetAllScansList,
                       new Dictionary<string, Action<DbParameter>>
                       {
                           { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                           { "@PermissionType", p => p.DbType = System.Data.DbType.String }
                       },
                       System.Data.CommandType.Text
                   );
            CommandBuilder commanCheckpointGroupdBuilder = new CommandBuilder(sqlGetScanCheckpointGroupByScanId,
                new Dictionary<string, Action<DbParameter>>
                      {
                            { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                      },
                      System.Data.CommandType.Text
                  );

            Scan scan = null;
            var scansList = new List<Scan>();

            using (var command = await commandBuilder.BuildFrom(_connection,
                        new Dictionary<string, object>
                        {
                            { "@UserGroupId", userGroupId},
                            { "@PermissionType", "Scan" }
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scan = new Scan
                        {
                            ScanId = Convert.ToInt32(reader["ScanId"].ToString()),
                            IsMonitor = Convert.ToBoolean(reader["IsMonitor"].ToString()),
                            BaseUrl = reader["BaseUrl"].ToString(),
                            DisplayName = reader["DisplayName"].ToString(),
                            IncludedDomains = reader["IncludedDomains"].ToString(),
                            IncludeFilter = reader["IncludeFilter"].ToString(),
                            ExcludeFilter = reader["ExcludeFilter"].ToString(),
                            UserAgent = reader["UserAgent"].ToString(),
                            IncludeMsofficeDocs = Convert.ToBoolean(reader["IncludeMsofficeDocs"].ToString()),
                            IncludeOtherDocs = Convert.ToBoolean(reader["IncludeOtherDocs"].ToString()),
                            RetestAllPages = Convert.ToBoolean(reader["RetestAllPages"].ToString()),
                            AlertMode = Convert.ToInt32(reader["AlertMode"].ToString()),
                            AlertDelay = Convert.ToInt32(reader["AlertDelay"].ToString()),
                            AlertSendTo = reader["AlertSendTo"].ToString(),
                            AlertSubject = reader["AlertSubject"].ToString(),
                            StartPages = Helpers.Helper.ConvertXmlStringtoObject<List<StartPage>>(reader["StartPages"].ToString()),
                            DateCreated = Convert.ToDateTime(reader["DateCreated"].ToString()),
                            DateModified = Convert.ToDateTime(reader["DateModified"].ToString()),
                            LocalClientId = reader["LocalClientId"].ToString(),
                            CanEdit = reader["ScanPermission"].ToString() == "edit" ? true : false
                        };
                        foreach (StartPage sPage in scan.StartPages)
                        {
                            scan.PageLimit = sPage.PageLimit;
                            scan.Levels = sPage.Levels;
                            scan.Path = sPage.Path;
                            scan.Script = sPage.Script;
                            scan.UserName = sPage.Username;
                            scan.Password = sPage.Password;
                            scan.Domain = sPage.Domain;
                            scan.PageLimit = sPage.PageLimit;
                            scan.WindowHeight = sPage.WindowHeight;
                            scan.WindowWidth = sPage.WindowWidth;
                        }
                        scansList.Add(scan);
                    }
                }
            }

            if (scansList.Count > 0)
            {
                foreach (Scan sc in scansList)
                {
                    List<string> chkGroupIds = new List<string>();
                    using (var command = await commanCheckpointGroupdBuilder.BuildFrom(_connection,
                                     new Dictionary<string, object>
                                     {
                                { "@ScanId", sc.ScanId}
                                     }, cancellationToken))
                    {
                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                chkGroupIds.Add(reader["CheckpointGroupId"].ToString());
                            }
                            sc.CheckpointGroupIds = chkGroupIds;
                        }
                    }
                }
            }
            return scansList;
        }

        public async Task<int> GetResultViewIdByScanId(int scanId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetResultViewIdByScanId,
               new Dictionary<string, Action<DbParameter>>
                   {
                        { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                   },
                    System.Data.CommandType.Text
               );

            int resultViewId = 0;

            using (var command = await commandBuilder.BuildFrom(_connection,
                             new Dictionary<string, object>
                             {
                                { "@ScanId", scanId }
                             }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        resultViewId = Convert.ToInt32(reader["ResultViewId"].ToString());
                    }
                }
            }

            return resultViewId;
        }

        public async Task<List<int>> GetScanIdList(int userGroupId, List<int> scanIds, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetScanIdList,
                 new Dictionary<string, Action<DbParameter>>
                       {
                         { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                         { "@PermissionType", p => p.DbType = System.Data.DbType.String },
                         { "@ScanIds", p => p.DbType = System.Data.DbType.String }
                       },
                       System.Data.CommandType.Text
                   );

            List<int> scanIdList = new List<int>();

            using (var command = await commandBuilder.BuildFrom(_connection,
                            new Dictionary<string, object>
                            {
                                { "@UserGroupId", userGroupId},
                                { "@PermissionType", "Scan" },
                                { "@ScanIds", string.Join(",", scanIds.ToArray())}
                            }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scanIdList.Add(Convert.ToInt32(reader["ScanId"].ToString()));
                    }
                }
            }

            return scanIdList;
        }

        public async Task<Scan> GetScanById(int scanId, int userGroupId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetScanByScanId,
                new Dictionary<string, Action<DbParameter>>
                      {
                         { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                         { "@PermissionType", p => p.DbType = System.Data.DbType.String },
                         { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                      },
                      System.Data.CommandType.Text
                  );

            CommandBuilder commanCheckpointGroupdBuilder = new CommandBuilder(sqlGetScanCheckpointGroupByScanId,
                new Dictionary<string, Action<DbParameter>>
                      {
                            { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                      },
                      System.Data.CommandType.Text
                  );
            Scan scan = null;
            List<string> chkGroupIds = new List<string>();
            using (var command = await commandBuilder.BuildFrom(_connection,
                            new Dictionary<string, object>
                            {
                                { "@UserGroupId", userGroupId},
                                { "@PermissionType", "Scan" },
                                { "@ScanId", scanId }
                            }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scan = new Scan
                        {
                            ScanId = Convert.ToInt32(reader["ScanId"].ToString()),
                            IsMonitor = Convert.ToBoolean(reader["IsMonitor"].ToString()),
                            BaseUrl = reader["BaseUrl"].ToString(),
                            DisplayName = reader["DisplayName"].ToString(),
                            IncludedDomains = reader["IncludedDomains"].ToString(),
                            IncludeFilter = reader["IncludeFilter"].ToString(),
                            ExcludeFilter = reader["ExcludeFilter"].ToString(),
                            UserAgent = reader["UserAgent"].ToString(),
                            IncludeMsofficeDocs = Convert.ToBoolean(reader["IncludeMsofficeDocs"].ToString()),
                            IncludeOtherDocs = Convert.ToBoolean(reader["IncludeOtherDocs"].ToString()),
                            RetestAllPages = Convert.ToBoolean(reader["RetestAllPages"].ToString()),
                            AlertMode = Convert.ToInt32(reader["AlertMode"].ToString()),
                            AlertDelay = Convert.ToInt32(reader["AlertDelay"].ToString()),
                            AlertSendTo = reader["AlertSendTo"].ToString(),
                            AlertSubject = reader["AlertSubject"].ToString(),
                            StartPages = Helpers.Helper.ConvertXmlStringtoObject<List<StartPage>>(reader["StartPages"].ToString()),
                            DateCreated = Convert.ToDateTime(reader["DateCreated"].ToString()),
                            DateModified = Convert.ToDateTime(reader["DateModified"].ToString()),
                            LocalClientId = reader["LocalClientId"].ToString(),
                            CanEdit = reader["ScanPermission"].ToString() == "edit" ? true : false
                        };
                        foreach (StartPage sPage in scan.StartPages)
                        {
                            scan.PageLimit = sPage.PageLimit;
                            scan.Levels = sPage.Levels;
                            scan.Path = sPage.Path;
                            scan.Script = sPage.Script;
                            scan.UserName = sPage.Username;
                            scan.Password = sPage.Password;
                            scan.Domain = sPage.Domain;
                            scan.PageLimit = sPage.PageLimit;
                            scan.WindowHeight = sPage.WindowHeight;
                            scan.WindowWidth = sPage.WindowWidth;
                        }
                    }
                }
            }

            if (scan != null)
            {
                using (var command = await commanCheckpointGroupdBuilder.BuildFrom(_connection,
                                 new Dictionary<string, object>
                                 {
                                { "@ScanId", scanId }
                                 }, cancellationToken))
                {
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            chkGroupIds.Add(reader["CheckpointGroupId"].ToString());
                        }
                        scan.CheckpointGroupIds = chkGroupIds;
                    }
                }
            }
            return scan;
        }

        public async Task<List<Scan>> GetScanByName(string scanName, CancellationToken cancellationToken)
        {
            scanName = "%" + WebUtility.UrlDecode(scanName) + "%";
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetScanDetailsByScanNameText,
                new Dictionary<string, Action<DbParameter>>
                      {
                            { "@ScanName", p => p.DbType = System.Data.DbType.String }
                      },
                      System.Data.CommandType.Text
                  );

            var scanList = new List<Scan>();

            using (var command = await commandBuilder.BuildFrom(_connection,
                        new Dictionary<string, object>
                        {
                            { "@ScanName", scanName},
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var scan = new Scan
                        {
                            ScanId = Convert.ToInt32(reader["ScanId"].ToString()),
                            DisplayName = reader["DisplayName"].ToString()
                        };
                        scanList.Add(scan);
                    }
                }
            }
            return scanList;
        }

        public async Task<IEnumerable<int>> GetAllScanIdsByScanGroupId(int scanGroupId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetAllScanIdsByScanGroupId,
                new Dictionary<string, Action<DbParameter>>
                      {
                            { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                      },
                      System.Data.CommandType.Text
                  );

            var scanIds = new List<int>();

            using (var command = await commandBuilder.BuildFrom(_connection,
                      new Dictionary<string, object>
                      {
                        { "@ScanGroupId", scanGroupId },
                      }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scanIds.Add(Convert.ToInt32(reader["ScanId"].ToString()));
                    }
                }
            }

            return scanIds;
        }

        public async Task<bool> CheckScanExistence(int scanId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlCheckScanExists,
               new Dictionary<string, Action<DbParameter>>
                   {
                        { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                   },
                    System.Data.CommandType.Text
               );

            bool scanExists = false;

            using (var command = await commandBuilder.BuildFrom(_connection,
                             new Dictionary<string, object>
                             {
                                { "@ScanId", scanId }
                             }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        scanExists = Convert.ToBoolean(reader["ScanExists"].ToString());
                    }
                }
            }

            return scanExists;
        }

        public string GetScanRecordForAudit(Scan scan)
        {
            string changes = string.Empty;

            changes += "Display Name : '" + scan.DisplayName + "'" + Environment.NewLine;
            changes += "URL : '" + scan.BaseUrl + "'" + Environment.NewLine;
            changes += "Include URLs matching : '" + scan.IncludeFilter + "'" + Environment.NewLine;
            changes += "Exclude URLs matching : '" + scan.ExcludeFilter + "'" + Environment.NewLine;

            string originalUserAgent = scan.UserAgent;
            if (string.IsNullOrEmpty(scan.UserAgent))
            {
                originalUserAgent = "(default)";
            }
            changes += "User Agent : '" + originalUserAgent + "'" + Environment.NewLine;
            changes += "Scan Office Documents : '" + scan.IncludeMsofficeDocs + "'" + Environment.NewLine;
            changes += "Include PDF files : '" + scan.IncludeOtherDocs + "'" + Environment.NewLine;
            changes += "Scan local content : '" + scan.LocalClientId + "'" + Environment.NewLine;
            changes += "Retest All Pages : '" + scan.RetestAllPages + "'" + Environment.NewLine;
            #region CheckpointGroups

            changes += "Checkpoint Groups:" + Environment.NewLine;
            List<string> checkpointGroupList = scan.CheckpointGroupIds.Cast<string>().ToList();
            foreach (string chkPointGrp in checkpointGroupList)
                changes += "-" + chkPointGrp + Environment.NewLine;
            #endregion

            #region IncludedDomains
            var includeDomainArray = scan.IncludedDomains.Split(TextFormatterService.includedDomainsSeparator, StringSplitOptions.RemoveEmptyEntries);
            List<string> includeDomainList = includeDomainArray.ToList<string>();
            if (includeDomainList.Count() > 0)
            {
                changes += "Included Domains:" + Environment.NewLine;
            }
            foreach (string domain in includeDomainList)
                changes += "-" + domain.Trim() + Environment.NewLine;
            #endregion

            #region StartPages

            changes += "Path : '" + scan.StartPages[0].Path + "'" + Environment.NewLine;
            changes += "Page Limit : '" + scan.StartPages[0].PageLimit + "'" + Environment.NewLine;
            changes += "Levels : '" + scan.StartPages[0].Levels + "'" + Environment.NewLine;
            changes += "Transaction Script : '" + scan.StartPages[0].Script + "'" + Environment.NewLine;
            changes += "UserName : '" + scan.StartPages[0].Username + "'" + Environment.NewLine;
            changes += "Domain : '" + scan.StartPages[0].Domain + "'" + Environment.NewLine;

            #endregion

            return changes;
        }

        public string GetScanChangesForAudit(Scan original, Scan updated, List<CheckpointGroupListItem> chkpointGroupList)
        {
            string changes = string.Empty;
            string tabSpaces = "\t";
            string newLine = "\n";
            string dash = "- ";

            var addedText = tabSpaces +"- Added :" + Environment.NewLine;
            var removedText = tabSpaces + "- Removed :" + Environment.NewLine;            
            bool firstIteration = true;

            if (original.DisplayName != updated.DisplayName) changes += "Display Name : changed from '" + original.DisplayName + "' to '" + updated.DisplayName + "'" + Environment.NewLine;
            if (original.BaseUrl != updated.BaseUrl) changes += "URL : changed from '" + original.BaseUrl + "' to '" + updated.BaseUrl + "'" + Environment.NewLine;
            if (original.IncludeFilter != updated.IncludeFilter) changes += "Include URLs matching : changed from '" + original.IncludeFilter + "' to '" + updated.IncludeFilter + "'" + Environment.NewLine;
            if (original.ExcludeFilter != updated.ExcludeFilter) changes += "Exclude URLs matching : changed from '" + original.ExcludeFilter + "' to '" + updated.ExcludeFilter + "'" + Environment.NewLine;

            string originalUserAgent = original.UserAgent;
            if (string.IsNullOrEmpty(original.UserAgent))
            {
                originalUserAgent = "(default)";
            }

            string updatedUserAgent = updated.UserAgent;
            if (string.IsNullOrEmpty(updated.UserAgent))
            {
                updatedUserAgent = "(default)";
            }

            if (original.UserAgent != updated.UserAgent) changes += "User Agent : changed from '" + originalUserAgent + "' to '" + updatedUserAgent + "'" + Environment.NewLine;
            if (original.IncludeMsofficeDocs != updated.IncludeMsofficeDocs) changes += "Scan Office Documents : changed from '" + original.IncludeMsofficeDocs + "' to '" + updated.IncludeMsofficeDocs + "'" + Environment.NewLine;
            if (original.IncludeOtherDocs != updated.IncludeOtherDocs) changes += "Include PDF files : changed from '" + original.IncludeOtherDocs + "' to '" + updated.IncludeOtherDocs + "'" + Environment.NewLine;
            if (original.LocalClientId != updated.LocalClientId) changes += "Scan local content : changed from '" + original.LocalClientId + "' to '" + updated.LocalClientId + "'" + Environment.NewLine;
            if (original.RetestAllPages != updated.RetestAllPages) changes += "Retest All Pages : changed from '" + original.RetestAllPages + "' to '" + updated.RetestAllPages + "'" + Environment.NewLine;

            #region CheckpointGroups

            List<string> originalCheckpointGroups = original.CheckpointGroupIds.Cast<string>().ToList();
            List<string> updatedCheckpointGroups = updated.CheckpointGroupIds.Cast<string>().ToList();

            var chkpointGroupDifferences = ComparisonServices.CompareLists<string>(originalCheckpointGroups, updatedCheckpointGroups);

            if (chkpointGroupDifferences > 0)
            {
                changes += "Checkpoint Groups:" + Environment.NewLine;
               
                foreach (string chkPointGrp in originalCheckpointGroups)
                {                    
                    if (!updatedCheckpointGroups.Contains(chkPointGrp))
                    {
                        if (firstIteration) changes += tabSpaces + "- Removed :" + Environment.NewLine; firstIteration = false;
                        var chkPointGroup = chkpointGroupList.Where(cp => cp.CheckpointGroupId == chkPointGrp).FirstOrDefault();
                        if (chkPointGrp != null)
                        {
                            changes += tabSpaces + tabSpaces + dash + chkPointGroup.ShortDescription + Environment.NewLine;
                        }                        
                    }
                }
                    
                firstIteration = true;

                foreach (string chkPointGrpAdded in updatedCheckpointGroups)
                {                    
                    if (!originalCheckpointGroups.Contains(chkPointGrpAdded))
                    {
                        if (firstIteration) changes += tabSpaces + "- Added :" + Environment.NewLine; firstIteration = false;
                        var chkPointGroup = chkpointGroupList.Where(cp => cp.CheckpointGroupId == chkPointGrpAdded).FirstOrDefault();

                        if(chkPointGroup != null)
                        {
                            changes += tabSpaces + tabSpaces + dash + chkPointGroup.ShortDescription + Environment.NewLine;
                        }                        
                    }                    
                }

                firstIteration = true;
            }

            #endregion

            #region IncludedDomains
            List<string> originalIncludeDomainList = original.IncludedDomains.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Cast<string>().ToList();
            List<string> updatedIncludeDomainList = updated.IncludedDomains.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Cast<string>().ToList();

            var includeDomainDifferences = ComparisonServices.CompareLists<string>(originalIncludeDomainList, updatedIncludeDomainList);

            if (includeDomainDifferences > 0)
            {
                changes += "Included Domains:" + Environment.NewLine;

                foreach (string domainRemoved in originalIncludeDomainList)
                {
                    if (!updatedIncludeDomainList.Contains(domainRemoved))
                    {
                        if (firstIteration) changes += tabSpaces + "- Removed :" + Environment.NewLine; firstIteration = false;
                        changes += tabSpaces + tabSpaces + dash + domainRemoved + newLine;
                    }
                }

                firstIteration = true;

                foreach (string domainAdded in updatedIncludeDomainList)
                {
                    if (!originalIncludeDomainList.Contains(domainAdded))
                    {
                        if (firstIteration) changes += tabSpaces + "- Added :" + Environment.NewLine; firstIteration = false;
                        changes += tabSpaces + tabSpaces + dash + domainAdded + newLine;
                    }
                }
            }

            #endregion

            #region StartPages

            if (original.StartPages[0].Path != updated.StartPages[0].Path) changes += "Path : changed from '" + original.StartPages[0].Path + "' to '" + updated.StartPages[0].Path + "'" + Environment.NewLine;
            if (original.StartPages[0].PageLimit != updated.StartPages[0].PageLimit) changes += "Page Limit : changed from '" + original.StartPages[0].PageLimit + "' to '" + updated.StartPages[0].PageLimit + "'" + Environment.NewLine;
            if (original.StartPages[0].Levels != updated.StartPages[0].Levels) changes += "Levels : changed from '" + original.StartPages[0].Levels + "' to '" + updated.StartPages[0].Levels + "'" + Environment.NewLine;
            if (original.StartPages[0].Script != updated.StartPages[0].Script) changes += "Transaction Script : changed from '" + original.StartPages[0].Script + "' to '" + updated.StartPages[0].Script + "'" + Environment.NewLine;
            if (original.StartPages[0].Username != updated.StartPages[0].Username) changes += "UserName : changed from '" + original.StartPages[0].Username + "' to '" + updated.StartPages[0].Username + "'" + Environment.NewLine;
            if (original.StartPages[0].Password != updated.StartPages[0].Password) changes += "Password is changed" + Environment.NewLine;
            if (original.StartPages[0].Domain != updated.StartPages[0].Domain) changes += "Domain : changed from '" + original.StartPages[0].Domain + "' to '" + updated.StartPages[0].Domain + "'" + Environment.NewLine;

            #endregion

            return changes;
        }

        public async Task<RecentScanResponse> GetRecentScans(RecentScanRequestModel request, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetRecentScans,
                       new Dictionary<string, Action<DbParameter>>
                       {
                           { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                           { "@PermissionType", p => p.DbType = System.Data.DbType.String },
                           { "@CurrentPage", p => p.DbType = System.Data.DbType.Int32 },
                           { "@RowsToFetch", p => p.DbType = System.Data.DbType.Int32 },
                           { "@SortColumn", p => p.DbType = System.Data.DbType.String },
                           { "@SortDirection", p => p.DbType = System.Data.DbType.String },
                       },
                       System.Data.CommandType.Text
                   );

            RecentScan recentScan = null;
            DateTime validStartDate;
            DateTime validFinishDate;
            var recentScanResponse = new RecentScanResponse();

            using (var command = await commandBuilder.BuildFrom(_connection,
                        new Dictionary<string, object>
                        {
                            { "@UserGroupId", request.UserGroupId},
                            { "@PermissionType", "Scan" },
                            { "@RowsToFetch", String.IsNullOrWhiteSpace(request.RecordsToReturn)? (object)DBNull.Value : Convert.ToInt32(request.RecordsToReturn) },
                            { "@CurrentPage", String.IsNullOrWhiteSpace(request.CurrentPage)? (object)DBNull.Value : Convert.ToInt32(request.CurrentPage) },
                            { "@SortColumn", string.IsNullOrWhiteSpace(request.SortColumn) ? (object)DBNull.Value : request.SortColumn },
                            { "@SortDirection", string.IsNullOrWhiteSpace(request.SortDirection) ? (object)DBNull.Value : request.SortDirection },
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        recentScan = new RecentScan
                        {
                            ScanId = !String.IsNullOrWhiteSpace(reader["ScanId"].ToString()) ? Convert.ToInt32(reader["ScanId"].ToString()) : 0,
                            RunId = !String.IsNullOrWhiteSpace(reader["RunId"].ToString()) ? Convert.ToInt32(reader["RunId"].ToString()) : (int?)null,
                            ScanName = reader["ScanName"].ToString(),
                            HealthScore = !String.IsNullOrWhiteSpace(reader["HealthScore"].ToString()) ? Convert.ToInt32(reader["HealthScore"].ToString()) : (int?)null,
                            CheckpointGroupDescription = reader["CheckpointGroupDescription"].ToString(),
                            CheckpointGroupId = reader["CheckpointGroupId"].ToString(),
                            Status = !String.IsNullOrWhiteSpace(reader["Status"].ToString()) ? Convert.ToInt32(reader["Status"].ToString()) : (int?)null,
                            Started = DateTime.TryParse(reader["Started"].ToString(), out validStartDate) ? validStartDate : (DateTime?)null,
                            Finished = DateTime.TryParse(reader["Finished"].ToString(), out validFinishDate) ? validFinishDate : (DateTime?)null,
                            StartingUrl = this._dataFormatterService.BuildUrl(reader["BaseUrl"].ToString(), reader["UrlPath"].ToString()),
                            TotalPagesRan = !String.IsNullOrWhiteSpace(reader["TotalPagesRan"].ToString()) ? Convert.ToInt32(reader["TotalPagesRan"].ToString()) : 0,
                            PageLimit = !String.IsNullOrWhiteSpace(reader["PageLimit"].ToString()) ? Convert.ToInt32(reader["PageLimit"].ToString()) : 0,
                            Levels = !String.IsNullOrWhiteSpace(reader["Levels"].ToString()) ? Convert.ToInt32(reader["Levels"].ToString()) : 0,
                            PreviousRunHealthScore = !String.IsNullOrWhiteSpace(reader["PreviousRunHealthScore"].ToString()) ? Convert.ToInt32(reader["PreviousRunHealthScore"].ToString()) : (int?)null,
                            HealthScoreChange = !String.IsNullOrWhiteSpace(reader["HealthScoreChange"].ToString()) ? Convert.ToInt32(reader["HealthScoreChange"].ToString()) : (int?)null,
                            HealthScoreChangePercent = !String.IsNullOrWhiteSpace(reader["HealthScoreChangePercent"].ToString()) ? Convert.ToDecimal(reader["HealthScoreChangePercent"].ToString()) : (decimal?)null,
                            ScannedLevels = !String.IsNullOrWhiteSpace(reader["ScannedLevels"].ToString()) ? Convert.ToInt32(reader["ScannedLevels"].ToString()) : 0
                        };

                        recentScanResponse.RecentScanList.Add(recentScan);
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        recentScanResponse.TotalRecords = Convert.ToInt32(reader["TotalScanRecords"].ToString());
                    }
                }
            }

            return recentScanResponse;
        }
    }
}
