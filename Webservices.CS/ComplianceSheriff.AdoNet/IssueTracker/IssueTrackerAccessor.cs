using ComplianceSheriff.Checkpoints;
using ComplianceSheriff.Configuration;
using ComplianceSheriff.AdoNet.Helpers;
using ComplianceSheriff.IssueTrackerReport;
using ComplianceSheriff.Licensing;
using ComplianceSheriff.Urls;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.IssueTracker
{
    public class IssueTrackerAccessor : IIssueTrackerAccessor
    {
        #region "SQL Queries"

            #region "Issue Tracker Results"
                private readonly string sqlRetrieveIssueTrackerResults = @"

                       DECLARE @TotalPagesImpacted int, @TotalOccurrences int, @SortColumnAndDirection varchar(2000), @OffsetRowCount int
                       DECLARE @TotalIssuesFound int, @TotalFailedIssues int, @TotalFilteredRecords int, @TotalUnfilteredRecords int
                       DECLARE @TotalPagesScanned int, @TotalHighSeverityFailedIssues int, @sql nvarchar(max), @PagingSQL varchar(4000), @AllowPaging bit

                       --DEFAULT FOR PAGING IS TRUE
                       SET @AllowPaging = 1

                       --DEFAULT TotalPagesScanned to 0
                       SET @TotalPagesScanned = 0

                       --DEFAULT TotalFilteredRecords to 0
					   SET @TotalFilteredRecords = 0

                       --IF @ImpactRange COMES AS NULL CHANGE TO EMPTY STRING
					   SET @ImpactRange = ISNULL(@ImpactRange , '')

                       IF @SortDirection IS NULL OR @SortDirection = ''
                           SET @SortDirection = 'ASC'

                       -- DEFAULTS TO Sorting By Severity Importance (High, Med, Low) then Impact
                       -- We included Issue in all sorts to keep rows consistent when returned
                       IF @SortColumn IS NULL OR @SortColumn = '' 
                           BEGIN
                               SET @SortColumnAndDirection = 'SeverityId ASC, Impact DESC, PriorityLevel ASC, Occurrences DESC, Issue ' + @SortDirection
                           END
                       ELSE
                           IF @SortColumn <> 'Issue'
                               BEGIN
                                   SET @SortColumnAndDirection = '[' + @SortColumn + '] ' + @SortDirection + ', Issue'
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

                       DECLARE @RunIds TABLE (
                          RunId int
                        )

                        DECLARE @CheckpointIds TABLE (
                          CheckpointId nvarchar(32)
                        )

                        DECLARE @CheckpointIdsByCheckpointGroupId TABLE (
                          CheckpointId nvarchar(32)
                        )

                        IF @ScanId IS NOT NULL
	                        BEGIN

		                        INSERT INTO @RunIds
                                SELECT MAX(RunId) AS RunId 
                                FROM Runs r
                                INNER JOIN dbo.udfScansByPermission(@UserGroupId, @ScanPermissionType) sgp
                                ON sgp.ScanId = r.ScanId
                                WHERE r.ScanId = @ScanId

	                        END

                        IF @ScanGroupId IS NOT NULL
	                        BEGIN

		                        INSERT INTO @RunIds
		                        SELECT RunId FROM
		                        (
			                        SELECT it.ScanId, MAX(RunId) As RunId 
                                    FROM reporting.IssueTracker it   
                                    INNER JOIN ScanGroupScans scs
                                      ON scs.ScanId = it.ScanId
                                    INNER JOIN [dbo].[udfScansByPermission] (@UserGroupId, @ScanPermissionType) sbp
                                      ON sbp.ScanId = it.ScanId
			                        WHERE scs.ScanGroupId IN (Select sgh.ScanGroupId FROM 
                                     [dbo].[udfScanGroupHierarchyByScanGroupId] (@ScanGroupId) sgh
                                     INNER JOIN [dbo].[udfScanGroupsByPermission] (@UserGroupId, @ScanGroupPermissionType) sgbp
                                       ON sgh.ScanGroupId = sgbp.ScanGroupId)
			                        GROUP BY it.ScanId
		                        ) a

                            END

                        --Populate CheckpointIds Table
                        INSERT INTO @CheckpointIds
                        EXEC [dbo].[GetAllCheckpointsBy_LicenseAndPermission] @LicensedModules, @UserGroupId

                        IF @CheckpointGroupId IS NOT NULL
                            BEGIN
                                INSERT INTO @CheckpointIdsByCheckpointGroupId
                                EXEC GetCheckpointsByCheckpointGroupId_LicenseAndPermission @CheckpointGroupId, @LicensedModules, @UserGroupId
                            END
                    
				        -- Non-filtered ResultSet (Only by ScanId or ScanGroupId)
                        SELECT *
				        INTO #TempIssueTracker 
                        FROM reporting.IssueTracker it
                        Where RunId IN (Select * FROM @RunIds)
                          AND (@CheckpointGroupId IS NULL OR CheckpointId IN (SELECT * FROM @CheckpointIdsByCheckpointGroupId))

                        SET @TotalUnfilteredRecords = (SELECT COUNT(1) FROM #TempIssueTracker)

                        -- SET Totals from unfiltered temp table
                        Select 
				                @TotalOccurrences = ISNULL(SUM(t.[COUNT]), 0),
						        @TotalPagesImpacted = COUNT(DISTINCT URL)
                        FROM #TempIssueTracker t

				        -- Total Pages Scanned from these Runs                
				        SET @TotalPagesScanned = (SELECT COUNT(URL) FROM Pages WHERE RunId IN (SELECT * FROM @RunIds) AND Pages.IsPage = 1)
                   

				        -- Get Total Issue Count
				        SET @TotalIssuesFound = (SELECT COUNT(1)
									            FROM
									            (
									                Select Issue
									                FROM #TempIssueTracker t
									                GROUP BY Issue, ValueId, CheckId
									            ) a )

                        SET @TotalFailedIssues = (SELECT COUNT(1)
									            FROM
									            (
									                Select Issue
									                FROM #TempIssueTracker t
                                                    WHERE [STATE] = 'Failed'
									                GROUP BY Issue, ValueId, CheckId
									            ) a )

                       -- Total High Severity Failed Issues from these Runs
				       SET @TotalHighSeverityFailedIssues = (SELECT COUNT(1)
									            FROM
									            (
												    SELECT Issue
													        ValueId, 
													        CheckId
									                 FROM #TempIssueTracker t
													 WHERE [STATE] = 'Failed'
													   AND (SELECT [dbo].[udfCalculateIssueSeverity](CheckpointPriority, PageLevel)) = 1 
									                GROUP BY Issue, ValueId, CheckId
									            ) a )
                    
                        -- Filter ResultSet
                        SELECT *				
				        INTO #TempIssueTrackerFiltered 
                        FROM #TempIssueTracker 
                        Where RunId IN (Select * FROM @RunIds)
                            AND (@CheckpointId IS NULL OR CheckpointId = @CheckpointId)
                            AND (@State IS NULL OR State IN (SELECT * FROM [dbo].[udf_STRING_SPLIT](@State, ',')))
                            AND (@Page IS NULL OR URL LIKE '%' + @Page + '%')
                            AND (@PriorityLevel IS NULL OR Priority IN (SELECT * FROM [dbo].[udf_STRING_SPLIT](@PriorityLevel, ',')))					

                        -- GROUP FILTERED RESULTSET
                        Select  Issue,
				                SUM(t.[COUNT]) AS Occurrences,
						        COUNT(DISTINCT URL) AS Pages,
						        COUNT(DISTINCT ScanId) AS Scans,
                                ValueId,
		                        t.CheckId
                        INTO #TempIssueGrouped
                        FROM #TempIssueTrackerFiltered t
				        GROUP BY Issue, ValueId, CheckId				
                    
                        Select Issue, Occurrences, Pages, Scans, 
                            (SELECT TOP 1 Number + ' ' + CheckpointShortDescription FROM #TempIssueTracker t2 WHERE t2.ValueId = t1.ValueId AND t2.CheckId = t1.CheckId) AS [Checkpoint],
					        (SELECT TOP 1 CheckpointPriority FROM #TempIssueTracker t2 WHERE t2.ValueId = t1.ValueId AND t2.CheckId = t1.CheckId ORDER BY CheckpointPriority) AS [PriorityLevel],
                            (SELECT TOP 1 PageLevel FROM #TempIssueTracker t2 WHERE t2.ValueId = t1.ValueId AND t2.CheckId = t1.CheckId ORDER BY PageLevel) AS [HighestPageLevel],
					        (SELECT TOP 1 State FROM #TempIssueTracker t2 WHERE t2.ValueId = t1.ValueId AND t2.CheckId = t1.CheckId) AS [State],
                            (SELECT TOP 1 CheckpointURL FROM #TempIssueTracker t2 WHERE t2.ValueId = t1.ValueId AND t2.CheckId = t1.CheckId) AS CheckpointURL,                            
                            CONVERT(DECIMAL(5,2), 100 * (CAST(t1.Pages as float) / CAST(@TotalPagesScanned as float))) AS Impact,
					        CheckId,
                            ValueId AS IssueId
                        INTO #TempCalculated
                        FROM #TempIssueGrouped t1

                        --Calculate Severity
				        SELECT  Issue, 
                                Occurrences,
                                Pages, 
                                Scans, 
                                [Checkpoint],
                                [PriorityLevel],
                                HighestPageLevel,
                                [State],
                                Impact,
						        CheckId,
                                IssueId,
                                CheckpointURL,
                                s.Severity,
                                c.SeverityId
				        INTO #TempComplete FROM
				        (
					        SELECT *, dbo.udfCalculateIssueSeverity([PriorityLevel], HighestPageLevel) AS SeverityId
					        FROM #TempCalculated
				        ) c
                        INNER JOIN IssueTrackerSeverity s
                            ON c.SeverityId = s.SeverityId
                        
                        SET @sql = 'SELECT * INTO ##TempCompleteFiltered FROM #TempComplete 
                                    WHERE (@Severity IS NULL OR Severity IN (SELECT * FROM [dbo].[udf_STRING_SPLIT](@Severity, '','')))' + @ImpactRange

                        exec sp_executeSQL @sql, N'@Severity varchar(50), @ImpactRange nvarchar(4000)', @Severity = @Severity, @ImpactRange = @ImpactRange

						IF OBJECT_ID('tempdb..##TempCompleteFiltered ') is not null
						BEGIN
                            SET @TotalFilteredRecords = (SELECT COUNT(1) FROM ##TempCompleteFiltered)                        

                            IF @AllowPaging = 1
                               BEGIN
				                 SET @sql = 'SELECT * FROM ##TempCompleteFiltered ORDER BY ' + @SortColumnAndDirection + @PagingSQL
                               END
                            ELSE
                               BEGIN
                                 SET @sql = 'SELECT * FROM ##TempCompleteFiltered ORDER BY ' + @SortColumnAndDirection
                               END

				            exec(@sql)

                            DROP TABLE ##TempCompleteFiltered
                        END

                        SET @sql = 'SELECT CheckpointId, Number + '' '' + ShortDescription AS CheckpointDescription FROM Checkpoints WHERE CheckpointId IN (SELECT DISTINCT [CheckId] FROM #TempIssueTracker) ORDER BY Number + '' '' + ShortDescription'
                        exec(@sql)

                        SELECT @TotalPagesScanned AS TotalPagesScanned,
                               @TotalPagesImpacted AS TotalPagesImpacted,
                               @TotalOccurrences AS TotalOccurrences,
                               @TotalIssuesFound AS TotalIssuesFound,
                               @TotalFilteredRecords AS TotalFilteredRecords,
                               @TotalFailedIssues AS TotalFailedIssues,
                               @TotalHighSeverityFailedIssues AS TotalHighSeverityFailedIssues

                        DROP TABLE #TempIssueTracker
                        DROP TABLE #TempIssueTrackerFiltered
                        DROP TABLE #TempIssueGrouped
                        DROP TABLE #TempCalculated
                        DROP TABLE #TempComplete";

        #endregion

            #region "Occurrences Query"
                
                private readonly string sqlOccurrencesQuery = @"

                       DECLARE @OffsetRowCount int, @SortColumnAndDirection varchar(max), @sql varchar(MAX), @TotalFilteredRecords int, @TotalOccurrences int

                       --DEFAULT TotalOccurrences to 0
                       SET @TotalOccurrences = 0

                       --DEFAULT TotalFilteredRecords to 0
					   SET @TotalFilteredRecords = 0

                       IF @SortDirection IS NULL OR @SortDirection = ''
                           SET @SortDirection = 'ASC'

                      -- Sorting                      
                       IF @SortColumn IS NULL OR @SortColumn = '' 
                           BEGIN
                               SET @SortColumnAndDirection = 'KeyAttrName, AttrValue DESC, PageTitle, [URL], [Line], [Column], ScanDisplayName, ScanId ' + @SortDirection
                           END
                       ELSE
                           BEGIN
                               SET @SortColumnAndDirection = '[' + @SortColumn + '] ' + @SortDirection
                           END

			           IF @CurrentPage = 1
			              BEGIN
					        SET @OffsetRowCount = 0
				          END
				        ELSE
				          BEGIN
					        SET @OffsetRowCount = (@CurrentPage-1) * @RowsToFetch
				          END

			            --Paging
				        DECLARE @PagingSQL varchar(4000)
			             SET @PagingSQL = ' OFFSET ' + CAST(@OffsetRowCount AS nvarchar(50)) + ' ROWS FETCH NEXT ' +  CAST(@RowsToFetch AS nvarchar(50)) + ' ROWS ONLY '

                        DECLARE @RunIds TABLE (
                                RunId int
                        )

                        DECLARE @CheckpointIds TABLE (
                            CheckpointId nvarchar(32)
                        )

                        IF @ScanId IS NOT NULL
	                        BEGIN

		                        INSERT INTO @RunIds
                                SELECT MAX(RunId) AS RunId FROM reporting.IssueTracker it
                                WHERE it.ScanId = @ScanId
	                        END

                        IF @ScanGroupId IS NOT NULL
	                        BEGIN

		                        INSERT INTO @RunIds
		                        SELECT RunId FROM
		                        (
			                        SELECT it.ScanId, MAX(RunId) As RunId 
                                    FROM reporting.IssueTracker it   
                                    INNER JOIN ScanGroupScans scs
                                      ON scs.ScanId = it.ScanId
                                    INNER JOIN [dbo].[udfScansByPermission] (@UserGroupId, @ScanPermissionType) sbp
                                      ON sbp.ScanId = it.ScanId
			                        WHERE scs.ScanGroupId IN (Select sgh.ScanGroupId FROM 
                                     [dbo].[udfScanGroupHierarchyByScanGroupId] (@ScanGroupId) sgh
                                     INNER JOIN [dbo].[udfScanGroupsByPermission] (@UserGroupId, @ScanGroupPermissionType) sgbp
                                       ON sgh.ScanGroupId = sgbp.ScanGroupId)
			                        GROUP BY it.ScanId
		                        ) a
                            END

                        IF @CheckpointGroupId IS NOT NULL
                            BEGIN
                                INSERT INTO @CheckpointIds
                                EXEC GetCheckpointsByCheckpointGroupId_LicenseAndPermission @CheckpointGroupId, @LicensedModules, @UserGroupId
                            END

                        Select  REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(it.Title)), CHAR(13), ''), CHAR(10), ''), CHAR(9), '') AS PageTitle,
		                        LTRIM(RTRIM(it.[Url])) AS [Url],
		                        Line,
                                ROW_NUMBER() OVER (PARTITION BY it.ResultId ORDER BY Line, [Col]) As OccurrenceIndex,
		                        Col AS [Column],
                                CASE WHEN LEN(KeyAttrName) = 0 THEN ' ' ELSE LTRIM(RTRIM(KeyAttrName)) END AS KeyAttrName,
                                LTRIM(RTRIM(AttrValue)) AS AttrValue,
                                it.ScanId,
		                        it.ScanDisplayName,
		                        it.ResultId,
                                it.[State],
                                LTRIM(RTRIM(riav.Element)) AS Element,
                                LTRIM(RTRIM(riav.ContainerId)) AS ContainerId,
                                it.CheckpointId
                        INTO #TempOccurrencesInitFiltered
                        FROM reporting.IssueTracker it
                          INNER JOIN [reporting].[ResultInstancesAttrValues] riav
                            ON it.ResultId = riav.ResultId
                        Where it.RunId IN (Select * FROM @RunIds)
                          AND ValueId = @IssueId
                          AND it.CheckpointId = @CheckpointId

                        SET @TotalOccurrences = (SELECT COUNT(1) FROM #TempOccurrencesInitFiltered)

                        SELECT DISTINCT PageTitle FROM #TempOccurrencesInitFiltered ORDER BY PageTitle

                        SELECT DISTINCT [URL] FROM #TempOccurrencesInitFiltered ORDER BY [URL]

                        SELECT DISTINCT CASE WHEN LEN(KeyAttrName) = 0 THEN ' ' ELSE KeyAttrName + '=' + AttrValue END AS KeyAttrNameValue FROM #TempOccurrencesInitFiltered ORDER BY KeyAttrNameValue

                        SELECT DISTINCT Element FROM #TempOccurrencesInitFiltered ORDER BY Element
   
                        SELECT DISTINCT ContainerId FROM #TempOccurrencesInitFiltered ORDER BY ContainerId

                        SELECT * INTO #TempOccurrencesSecondaryFiltered
                        FROM #TempOccurrencesInitFiltered
                        WHERE (@CheckpointGroupId IS NULL OR CheckpointId IN (SELECT * FROM @CheckpointIds))
                          AND (@ScanName IS NULL OR ScanDisplayName = @ScanName)
                          AND (@PageTitle IS NULL OR (@PageTitle = '[NULL]' AND PageTitle IS NULL) OR PageTitle = @PageTitle) 
                          AND (@PageURL IS NULL OR [URL] = @PageUrl)
                          AND (@KeyAttributeNameValue IS NULL OR (@KeyAttributeNameValue = KeyAttrName OR KeyAttrName + '=' + AttrValue = @KeyAttributeNameValue))  
                          AND (@Element IS NULL OR Element = @Element)  
                          AND (@ContainerId IS NULL OR ContainerId = @ContainerId)
                          AND (@State IS NULL OR [State] = @State)

                        SET @TotalFilteredRecords = (SELECT COUNT(1) FROM #TempOccurrencesSecondaryFiltered)

                        SET @sql = 'SELECT * FROM #TempOccurrencesSecondaryFiltered ORDER BY ' + @SortColumnAndDirection + ' ' + @PagingSQL
                        PRINT @sql
                        exec(@sql)

                       SELECT  @TotalOccurrences AS TotalOccurrences,
                               @TotalFilteredRecords AS TotalFilteredRecords

                        DROP TABLE #TempOccurrencesInitFiltered;
                        DROP TABLE #TempOccurrencesSecondaryFiltered";

        #endregion

            #region "Occurrences By Page Query"

                private readonly string occurrencesByPageQuery = @"
            
                       DECLARE @OffsetRowCount int, @SortColumnAndDirection varchar(max)
                       DECLARE @sql varchar(max), @TotalFilteredPages int, @TotalPages int

                       --DEFAULT TotalPages to 0
                       SET @TotalPages = 0

                       --DEFAULT TotalFilteredPages to 0
					   SET @TotalFilteredPages = 0

                      IF @SortDirection IS NULL OR @SortDirection = ''
                           SET @SortDirection = 'ASC'

                      -- Sorting                      
                       IF @SortColumn IS NULL OR @SortColumn = '' 
                           BEGIN
                               SET @SortColumnAndDirection = 'NoOfOccurrences DESC, [URL] ' + @SortDirection
                           END
                       ELSE
                           BEGIN
                               SET @SortColumnAndDirection = @SortColumn + ' ' + @SortDirection
                           END

			           IF @CurrentPage = 1
			              BEGIN
					        SET @OffsetRowCount = 0
				          END
				        ELSE
				          BEGIN
					        SET @OffsetRowCount = (@CurrentPage-1) * @RowsToFetch
				          END

			            --Paging
				        DECLARE @PagingSQL varchar(4000)
			             SET @PagingSQL = ' OFFSET ' + CAST(@OffsetRowCount AS nvarchar(50)) + ' ROWS FETCH NEXT ' +  CAST(@RowsToFetch AS nvarchar(50)) + ' ROWS ONLY '

                        DECLARE @RunIds TABLE (
                                RunId int
                        )

                        DECLARE @CheckpointIds TABLE (
                            CheckpointId nvarchar(32)
                        )

                        IF @ScanId IS NOT NULL
	                        BEGIN

		                        INSERT INTO @RunIds
                                SELECT MAX(RunId) AS RunId FROM reporting.IssueTracker it
                                WHERE it.ScanId = @ScanId
	                        END

                        IF @ScanGroupId IS NOT NULL
	                        BEGIN

		                        INSERT INTO @RunIds
		                        SELECT RunId FROM
		                        (
			                        SELECT it.ScanId, MAX(RunId) As RunId 
                                    FROM reporting.IssueTracker it   
                                    INNER JOIN ScanGroupScans scs
                                      ON scs.ScanId = it.ScanId
                                    INNER JOIN [dbo].[udfScansByPermission] (@UserGroupId, @ScanPermissionType) sbp
                                      ON sbp.ScanId = it.ScanId
			                        WHERE scs.ScanGroupId IN (Select sgh.ScanGroupId FROM 
                                     [dbo].[udfScanGroupHierarchyByScanGroupId] (@ScanGroupId) sgh
                                     INNER JOIN [dbo].[udfScanGroupsByPermission] (@UserGroupId, @ScanGroupPermissionType) sgbp
                                       ON sgh.ScanGroupId = sgbp.ScanGroupId)
			                        GROUP BY it.ScanId
		                        ) a
                            END

                        IF @CheckpointGroupId IS NOT NULL
                            BEGIN
                                INSERT INTO @CheckpointIds
                                EXEC GetCheckpointsByCheckpointGroupId_LicenseAndPermission @CheckpointGroupId, @LicensedModules, @UserGroupId
                            END

                        Select  REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(it.Title)), CHAR(13), ''), CHAR(10), ''), CHAR(9), '') AS PageTitle,
		                        it.[Url],
                                it.RunId,
                                ROW_NUMBER() OVER (PARTITION BY it.ResultId ORDER BY Line, [Col]) As OccurrenceIndex,
		                        Line,
		                        Col AS [Column],
		                        KeyAttrName,
                                AttrValue,
                                it.ScanId,
		                        it.ScanDisplayName,
		                        it.ResultId,
                                it.ResultTextId,
                                it.[State],
                                riav.Element,
								riav.CheckPointId,
                                riav.ContainerId
                        INTO #TempOccurrences
                        FROM reporting.IssueTracker it
                          LEFT OUTER JOIN [reporting].[ResultInstancesAttrValues] riav
                            ON it.ResultId = riav.ResultId
                        Where (@CheckpointGroupId IS NULL OR CheckId IN (SELECT * FROM @CheckpointIds))
                          AND it.RunId IN (Select * FROM @RunIds)
                          AND ValueId = @IssueId
                          AND (riav.CheckpointId IS NULL OR riav.CheckpointId = @CheckpointId)
                          AND (@CheckpointId IS NULL OR it.CheckpointId = @CheckpointId)

						SELECT  Url
                        INTO #TotalPagesTemp FROM #TempOccurrences
						GROUP BY Url

						SET @TotalPages = (SELECT COUNT(DISTINCT URL) FROM #TotalPagesTemp)

						DROP TABLE #TotalPagesTemp

                        SELECT * INTO #TempOccurrencesFiltered FROM #TempOccurrences
                          WHERE (@ScanName IS NULL OR ScanDisplayName = @ScanName)
                            AND (@PageTitle IS NULL OR PageTitle = @PageTitle) 
                            AND (@PageURL IS NULL OR [URL] = @PageUrl)
                            AND (@KeyAttributeNameValue IS NULL OR (@KeyAttributeNameValue = KeyAttrName OR KeyAttrName + '=' + AttrValue = @KeyAttributeNameValue))  
                            AND (@Element IS NULL OR Element = @Element)  
                            AND (@ContainerId IS NULL OR ContainerId = @ContainerId)
                            AND (@State IS NULL OR [State] = @State)                       

						SELECT  Url, 
								ROW_NUMBER() OVER(ORDER BY Url ASC) AS RowNumber,
                                COUNT(CheckpointId) AS NoOfOccurrences,
                                MAX(PageTitle) AS PageTitle,
                                MAX(RunId) As RunId,
                                MAX(ResultTextId) As ResultTextId,
                                MAX(CheckpointId) AS CheckpointId,
                                MAX(ResultId) AS ResultId
                        INTO #Pages FROM #TempOccurrencesFiltered
						GROUP BY Url

                        SET @TotalFilteredPages = (SELECT COUNT(1) FROM #Pages)

				        SET @sql = 'SELECT * FROM #Pages ORDER BY ' + @SortColumnAndDirection + ' ' + @PagingSQL
				        exec(@sql)

						Select * FROM #Pages p
						 INNER JOIN #TempOccurrencesFiltered o
						    ON p.Url = o.Url
                           AND p.CheckpointId = o.CheckPointId
						ORDER BY p.RowNumber

                       SELECT  @TotalPages AS TotalPages,
                               @TotalFilteredPages AS TotalFilteredPages

						DROP TABLE #TempOccurrences
                        DROP TABLE #TempOccurrencesFiltered
						DROP TABLE #Pages";
        #endregion

            #region "Occurrences Export Query"

                private readonly string occurrencesExportquery = @"

                     DECLARE @OffsetRowCount int, @SortColumnAndDirection varchar(max), @CheckpointGroupName varchar(1000)
                     DECLARE @sql varchar(max), @PagingSQL varchar(4000), @AllowPaging bit

                     IF @SortDirection IS NULL OR @SortDirection = ''
                           SET @SortDirection = 'ASC'

                      -- Sorting                      
                       IF @SortColumn IS NULL OR @SortColumn = '' 
                           BEGIN
                               SET @SortColumnAndDirection = ' Url, KeyAttrName ' + @SortDirection
                           END
                       ELSE
                           BEGIN
                               SET @SortColumnAndDirection = @SortColumn + ' ' + @SortDirection
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

                        DECLARE @RunIds TABLE (
                                RunId int
                        )

                        DECLARE @CheckpointIds TABLE (
                            CheckpointId nvarchar(32)
                        )

                        DECLARE @CheckpointGroupIds AS [dbo].[CheckpointGroupIdsTableType];

                        IF @ScanId IS NOT NULL
	                        BEGIN

		                        INSERT INTO @RunIds
                                SELECT MAX(RunId) AS RunId FROM reporting.IssueTracker it
                                WHERE it.ScanId = @ScanId

                                INSERT INTO @CheckpointGroupIds(CheckpointGroupId)
                                SELECT scpg.[CheckpointGroupId]
                                  FROM [dbo].[ScanCheckpointGroups] scpg
                                WHERE ScanId = @ScanId
	                        END

                        IF @ScanGroupId IS NOT NULL
	                        BEGIN

		                        INSERT INTO @RunIds
		                        SELECT RunId FROM
		                        (
			                        SELECT it.ScanId, MAX(RunId) As RunId 
                                    FROM reporting.IssueTracker it   
                                    INNER JOIN ScanGroupScans scs
                                      ON scs.ScanId = it.ScanId
                                    INNER JOIN [dbo].[udfScansByPermission] (@UserGroupId, @ScanPermissionType) sbp
                                      ON sbp.ScanId = it.ScanId
			                        WHERE scs.ScanGroupId IN (Select sgh.ScanGroupId FROM 
                                     [dbo].[udfScanGroupHierarchyByScanGroupId] (@ScanGroupId) sgh
                                     INNER JOIN [dbo].[udfScanGroupsByPermission] (@UserGroupId, @ScanGroupPermissionType) sgbp
                                       ON sgh.ScanGroupId = sgbp.ScanGroupId)
			                        GROUP BY it.ScanId
		                        ) a

                                INSERT INTO @CheckpointGroupIds(CheckpointGroupId)
                                SELECT DISTINCT CheckpointGroupId FROM ScanCheckpointGroups
                                WHERE Scanid In (SELECT ScanId FROM [dbo].[ScanGroupScans] Where ScanGroupId = @ScanGroupId)
                            END

                        IF @CheckpointGroupId IS NOT NULL
                            BEGIN
                                INSERT INTO @CheckpointIds
                                EXEC GetCheckpointsByCheckpointGroupId_LicenseAndPermission @CheckpointGroupId, @LicensedModules, @UserGroupId

                                SET @CheckpointGroupName = (SELECT ShortDescription FROM CheckpointGroups Where CheckpointGroupId = @CheckpointGroupId)
                            END

                        Select  REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(it.Title)), CHAR(13), ''), CHAR(10), ''), CHAR(9), '') AS PageTitle,
		                        it.[Url],
								riav.Element,
								riav.ContainerId,
		                        KeyAttrName,
                                AttrValue,
								it.Issue,
								sg.DisplayName AS ScanGroupName,
								it.ScanDisplayName,
                                it.ScanId,
								@CheckpointGroupName AS CheckpointGroupName,
								it.CheckpointShortDescription,
								it.ResultTextId,
								it.RunId,
                                it.[State],
								riav.CheckPointId
                        INTO #TempOccurrences
                        FROM reporting.IssueTracker it
                          INNER JOIN [reporting].[ResultInstancesAttrValues] riav
                            ON it.ResultId = riav.ResultId
						  LEFT OUTER JOIN ScanGroups sg
						    ON it.ScanGroupId = sg.ScanGroupId
                        Where (@CheckpointGroupId IS NULL OR CheckId IN (SELECT * FROM @CheckpointIds))
                          AND it.RunId IN (Select * FROM @RunIds)
                          AND (@IssueId IS NULL OR ValueId = @IssueId)
                          AND (@CheckpointId IS NULL OR riav.CheckpointId = @CheckpointId)
                          AND (@ScanName IS NULL OR it.ScanDisplayName = @ScanName)
                          AND (@PageTitle IS NULL OR it.Title = @PageTitle) 
                          AND (@PageURL IS NULL OR it.[URL] = @PageUrl)
                          AND (@KeyAttributeNameValue IS NULL OR (@KeyAttributeNameValue = KeyAttrName OR KeyAttrName + '=' + AttrValue = @KeyAttributeNameValue))  
                          AND (@Element IS NULL OR Element = @Element)  
                          AND (@ContainerId IS NULL OR ContainerId = @ContainerId)
                          AND (@State IS NULL OR it.[State] = @State)

                        IF @AllowPaging = 1
                           BEGIN
				             SET @sql = 'SELECT * FROM #TempOccurrences ORDER BY ' + @SortColumnAndDirection + ' ' + @PagingSQL
                           END
                        ELSE
                           BEGIN
                             SET @sql = 'SELECT * FROM #TempOccurrences ORDER BY ' + @SortColumnAndDirection
                           END

				        exec(@sql)

						DROP TABLE #TempOccurrences";
            #endregion

        #endregion


        private readonly IConnectionManager connection;
        private readonly ConfigurationOptions _configOptions;
        private readonly ILogger<IssueTrackerAccessor> _logger;
        private readonly ILicensingService _licensingService;
        private readonly IUrlServices _urlService;

        public IssueTrackerAccessor(IConnectionManager connection,
                                    IOptions<ConfigurationOptions> options,
                                    ILogger<IssueTrackerAccessor> logger,
                                    IUrlServices urlService,
                                    ILicensingService licensingService)
        {
            _configOptions = options.Value;
            this.connection = connection;
            _urlService = urlService;
            _licensingService = licensingService;            
            _logger = logger;
        }

        public async Task<OccurrencesResponse> GetOccurrences(int userGroupId, string organizationId, OccurrencesRequest request, CancellationToken cancellationToken)
        {
                CommandBuilder commandBuilder = new CommandBuilder(sqlOccurrencesQuery,
                    new Dictionary<string, Action<DbParameter>>
                    {
                        { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@IssueId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@CheckpointId", p => p.DbType = System.Data.DbType.String },
                        { "@ScanGroupPermissionType", p => p.DbType = System.Data.DbType.String },
                        { "@ScanPermissionType", p => p.DbType = System.Data.DbType.String },
                        { "@LicensedModules", p => p.DbType = System.Data.DbType.String },
                        { "@CheckpointGroupId", p => p.DbType = System.Data.DbType.String },
                        { "@State", p => p.DbType = System.Data.DbType.String },
                        { "@PageTitle", p => p.DbType = System.Data.DbType.String },
                        { "@PageUrl", p => p.DbType = System.Data.DbType.String },
                        { "@KeyAttributeNameValue", p => p.DbType = System.Data.DbType.String },
                        { "@Element", p => p.DbType = System.Data.DbType.String },
                        { "@ContainerId", p => p.DbType = System.Data.DbType.String },
                        { "@ScanName", p => p.DbType = System.Data.DbType.String },
                        { "@CurrentPage", p => p.DbType = System.Data.DbType.Int32 },
                        { "@RowsToFetch", p => p.DbType = System.Data.DbType.Int32 },
                        { "@SortColumn", p => p.DbType = System.Data.DbType.String },
                        { "@SortDirection", p => p.DbType = System.Data.DbType.String },                        
                    },
                    System.Data.CommandType.Text
                );

                var occurrencesResponse = new OccurrencesResponse();

                var licensedModules = _licensingService.GetLicensedModuleString(organizationId, _configOptions);

                using (var command = await commandBuilder.BuildFrom(connection,
                            new Dictionary<string, object>
                            {
                                { "@UserGroupId", userGroupId},
                                { "@ScanGroupPermissionType", "ScanGroup" },
                                { "@ScanPermissionType", "Scan" },
                                { "@ScanId", string.IsNullOrWhiteSpace(request.ScanId) ? (object)DBNull.Value : Convert.ToInt32(request.ScanId) },
                                { "@ScanGroupId", string.IsNullOrWhiteSpace(request.ScanGroupId) ? (object)DBNull.Value : Convert.ToInt32(request.ScanGroupId) },
                                { "@IssueId", Convert.ToInt32(request.IssueId) },
                                { "@CheckpointId",  request.CheckpointId },
                                { "@LicensedModules",  licensedModules},
                                { "@CheckpointGroupId", string.IsNullOrWhiteSpace(request.CheckpointGroupId) ? (object)DBNull.Value : request.CheckpointGroupId },
                                { "@ScanName", string.IsNullOrWhiteSpace(request.ScanName) ? (object)DBNull.Value : request.ScanName },    
                                { "@State", string.IsNullOrWhiteSpace(request.State) ? (object)DBNull.Value : request.State },
                                { "@PageTitle", request.PageTitle ?? (object)DBNull.Value },
                                { "@PageUrl", string.IsNullOrWhiteSpace(request.PageUrl) ? (object)DBNull.Value : request.PageUrl },
                                { "@KeyAttributeNameValue", request.KeyAttribute ?? (object)DBNull.Value },
                                { "@Element", request.Element ?? (object)DBNull.Value },
                                { "@ContainerId", request.ContainerId ?? (object)DBNull.Value },
                                { "@RowsToFetch", request.RecordsToReturn },
                                { "@CurrentPage", request.CurrentPage },
                                { "@SortColumn", string.IsNullOrWhiteSpace(request.SortColumn) ? (object)DBNull.Value : request.SortColumn },
                                { "@SortDirection", string.IsNullOrWhiteSpace(request.SortDirection) ? (object)DBNull.Value : request.SortDirection },
                            }, cancellationToken))
                {
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            string pageTitle = reader["PageTitle"] == DBNull.Value ? "[NULL]" : reader["PageTitle"].ToString();

                            var kvpPageTitle = new KeyValuePair<string, string>(
                                pageTitle,
                                String.IsNullOrWhiteSpace(reader["PageTitle"].ToString()) ? "(no value)" : pageTitle);

                            occurrencesResponse.TitleFilterList.Add(kvpPageTitle);
                        };

                        reader.NextResult();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            occurrencesResponse.UrlFilterList.Add(reader["URL"].ToString());
                        };

                        reader.NextResult();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                        var kvpKeyAttrNameValue = new KeyValuePair<string, string>(
                            reader["KeyAttrNameValue"].ToString(),
                            String.IsNullOrWhiteSpace(reader["KeyAttrNameValue"].ToString()) ? "(no value)" : reader["KeyAttrNameValue"].ToString());

                            occurrencesResponse.KeyAttributeFilterList.Add(kvpKeyAttrNameValue);
                        };

                        reader.NextResult();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                        var kvpElement = new KeyValuePair<string, string>(
                            reader["Element"].ToString(),
                            String.IsNullOrWhiteSpace(reader["Element"].ToString()) ? "(no value)" : reader["Element"].ToString());

                            occurrencesResponse.ElementFilterList.Add(kvpElement);
                        };

                        reader.NextResult();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                        var kvpContainerId = new KeyValuePair<string, string>(
                            reader["ContainerId"].ToString(),
                            String.IsNullOrWhiteSpace(reader["ContainerId"].ToString()) ? "(no value)" : reader["ContainerId"].ToString());

                            occurrencesResponse.ContainerIdFilterList.Add(kvpContainerId);
                        };

                        reader.NextResult();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            occurrencesResponse.OccurrencesList.Add(new OccurrenceItem()
                            {
                                PageTitle = reader["PageTitle"].ToString(),
                                PageUrl = reader["URL"].ToString(),
                                LineNumber = Convert.ToInt32(reader["Line"].ToString()),
                                ColumnNumber = Convert.ToInt32(reader["Column"].ToString()),
                                KeyAttribute = !String.IsNullOrWhiteSpace(reader["KeyAttrName"].ToString()) ? String.Format("{0}={1}", reader["KeyAttrName"].ToString(), reader["AttrValue"].ToString()) : "",
                                State = reader["State"].ToString(),
                                ScanId = Convert.ToInt32(reader["ScanId"].ToString()),
                                Element = reader["Element"].ToString(),
                                ContainerId = reader["ContainerId"].ToString(),
                                ScanDisplayName = reader["ScanDisplayName"].ToString(),
                                ResultId = Convert.ToInt32(reader["ResultId"].ToString()),
                                CachedPageUrl = String.Format("{0}/ShowInstances.aspx?resultId={1}&occurrenceIndex={2}", request.ReferrerUrl, Convert.ToInt32(reader["ResultId"].ToString()), Convert.ToInt32(reader["OccurrenceIndex"].ToString()))
                            });
                        }

                        reader.NextResult();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            occurrencesResponse.TotalOccurrences = Convert.ToInt32(reader["TotalOccurrences"].ToString());
                            occurrencesResponse.TotalFilteredRecords = Convert.ToInt32(reader["TotalFilteredRecords"].ToString());
                        }
                    }
                }

            return occurrencesResponse;
        }

        public async Task<OccurrencesByPageResponse> GetOccurrencesByPage(int userGroupId, string organizationId, OccurrencesByPageRequest request, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(occurrencesByPageQuery,
                                new Dictionary<string, Action<DbParameter>>
                                {
                                    { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@IssueId", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@CheckpointId", p => p.DbType = System.Data.DbType.String },
                                    { "@ScanGroupPermissionType", p => p.DbType = System.Data.DbType.String },
                                    { "@ScanPermissionType", p => p.DbType = System.Data.DbType.String },
                                    { "@LicensedModules", p => p.DbType = System.Data.DbType.String },
                                    { "@CheckpointGroupId", p => p.DbType = System.Data.DbType.String },
                                    { "@State", p => p.DbType = System.Data.DbType.String },
                                    { "@PageTitle", p => p.DbType = System.Data.DbType.String },
                                    { "@PageUrl", p => p.DbType = System.Data.DbType.String },
                                    { "@KeyAttributeNameValue", p => p.DbType = System.Data.DbType.String },                                    
                                    { "@Element", p => p.DbType = System.Data.DbType.String },
                                    { "@ContainerId", p => p.DbType = System.Data.DbType.String },
                                    { "@ScanName", p => p.DbType = System.Data.DbType.String },
                                    { "@CurrentPage", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@RowsToFetch", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@SortColumn", p => p.DbType = System.Data.DbType.String },
                                    { "@SortDirection", p => p.DbType = System.Data.DbType.String },                        
                                },
                                System.Data.CommandType.Text
                            );

            var occurrencesByPageResponse = new OccurrencesByPageResponse();  //new List<OccurrencePage>();
            var occurrencePageItems = new List<OccurrencePageItem>();

            var licensedModules = _licensingService.GetLicensedModuleString(organizationId, _configOptions);

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@UserGroupId", userGroupId},
                            { "@ScanGroupPermissionType", "ScanGroup" },
                            { "@ScanPermissionType", "Scan" },
                            { "@ScanId", string.IsNullOrWhiteSpace(request.ScanId) ? (object)DBNull.Value : Convert.ToInt32(request.ScanId) },
                            { "@ScanGroupId", string.IsNullOrWhiteSpace(request.ScanGroupId) ? (object)DBNull.Value : Convert.ToInt32(request.ScanGroupId) },
                            { "@IssueId", Convert.ToInt32(request.IssueId) },
                            { "@CheckpointId",  request.CheckpointId },
                            { "@LicensedModules",  licensedModules},
                            { "@CheckpointGroupId", string.IsNullOrWhiteSpace(request.CheckpointGroupId) ? (object)DBNull.Value : request.CheckpointGroupId },
                            { "@ScanName", string.IsNullOrWhiteSpace(request.ScanName) ? (object)DBNull.Value : request.ScanName },
                            { "@State", string.IsNullOrWhiteSpace(request.State) ? (object)DBNull.Value : request.State },
                            { "@PageTitle", request.PageTitle ?? (object)DBNull.Value },
                            { "@PageUrl", string.IsNullOrWhiteSpace(request.PageUrl) ? (object)DBNull.Value : request.PageUrl },
                            { "@KeyAttributeNameValue", request.KeyAttribute ?? (object)DBNull.Value },
                            { "@Element", request.Element ?? (object)DBNull.Value },
                            { "@ContainerId", request.ContainerId ?? (object)DBNull.Value },
                            { "@RowsToFetch", request.RecordsToReturn },
                            { "@CurrentPage", request.CurrentPage },                                                              
                            { "@SortColumn", string.IsNullOrWhiteSpace(request.SortColumn) ? (object)DBNull.Value : request.SortColumn },
                            { "@SortDirection", string.IsNullOrWhiteSpace(request.SortDirection) ? (object)DBNull.Value : request.SortDirection },
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        occurrencesByPageResponse.OccurrencePages.Add(new OccurrencePage
                        {
                            RowNumber = Convert.ToInt32(reader["RowNumber"].ToString()),
                            PageTitle = reader["PageTitle"].ToString(),
                            NoOfOccurrences = Convert.ToInt32(reader["NoOfOccurrences"].ToString()),
                            PageUrl = reader["Url"].ToString(),
                            CachedPageLink = String.Format("{0}/ShowInstances.aspx?resultId={1}",
                                                                request.ReferrerUrl,
                                                                Convert.ToInt32(reader["ResultId"].ToString()))
                        });
                    }

                    reader.NextResult();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        occurrencePageItems.Add(new OccurrencePageItem
                        {
                            RowNumber = !string.IsNullOrWhiteSpace(reader["RowNumber"].ToString()) ? Convert.ToInt32(reader["RowNumber"].ToString()) : 0,
                            ScanId = Convert.ToInt32(reader["ScanId"].ToString()),
                            ScanDisplayName = reader["ScanDisplayName"].ToString(),
                            LineNumber = !string.IsNullOrWhiteSpace(reader["Line"].ToString()) ?  (int?)Convert.ToInt32(reader["Line"].ToString()) : null,
                            ColumnNumber = !string.IsNullOrWhiteSpace(reader["Column"].ToString()) ? (int?)Convert.ToInt32(reader["Column"].ToString()) : null,
                            KeyAttribute = !string.IsNullOrWhiteSpace(reader["KeyAttrName"].ToString()) ? String.Format("{0}={1}", reader["KeyAttrName"].ToString(), reader["AttrValue"].ToString()) : "",
                            PageUrl = reader["Url"].ToString(),
                            State = reader["State"].ToString(),                            
                            Element = reader["Element"].ToString(),
                            ContainerId = reader["ContainerId"].ToString(),
                            CachedPageLink = String.Format("{0}/ShowInstances.aspx?resultId={1}&occurrenceIndex={2}",
                                                                request.ReferrerUrl,
                                                                reader["ResultId"].ToString(),
                                                                reader["OccurrenceIndex"].ToString()),
                            ResultId = !String.IsNullOrWhiteSpace(reader["ResultId"].ToString()) ? Convert.ToInt32(reader["ResultId"].ToString()) : 0
                        });
                    }

                    reader.NextResult();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        occurrencesByPageResponse.TotalPages = Convert.ToInt32(reader["TotalPages"].ToString());
                        occurrencesByPageResponse.TotalFilteredPages = Convert.ToInt32(reader["TotalFilteredPages"].ToString());
                    }
                }

                foreach (var page in occurrencesByPageResponse.OccurrencePages)
                {
                    page.Occurrences.AddRange(occurrencePageItems
                                                .Where(p => p.PageUrl == page.PageUrl)
                                                .ToList()
                                                .OrderBy(p => p.PageUrl)
                                                .ThenBy(s => s.ScanDisplayName)
                                                .ThenBy(l => l.LineNumber)
                                                .ThenBy(c => c.ColumnNumber));
                }
            }

            return occurrencesByPageResponse;
        }

        public async Task<IssueTrackerResponse> GetIssueTrackerResults(int userGroupId, string organizationId, IssueTrackerRequestBase request, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlRetrieveIssueTrackerResults,
                       new Dictionary<string, Action<DbParameter>>
                       {
                           { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                           { "@ScanGroupPermissionType", p => p.DbType = System.Data.DbType.String },
                           { "@ScanPermissionType", p => p.DbType = System.Data.DbType.String },
                           { "@CheckpointPermissionType", p => p.DbType = System.Data.DbType.String },
                           { "@LicensedModules", p => p.DbType = System.Data.DbType.String },
                           { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
                           { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                           { "@CurrentPage", p => p.DbType = System.Data.DbType.Int32 },
                           { "@RowsToFetch", p => p.DbType = System.Data.DbType.Int32 },
                           { "@CheckpointGroupId", p => p.DbType = System.Data.DbType.String },
                           { "@CheckpointId", p => p.DbType = System.Data.DbType.String },
                           { "@State", p => p.DbType = System.Data.DbType.String },
                           { "@Page", p => p.DbType = System.Data.DbType.String },
                           { "@PriorityLevel", p => p.DbType = System.Data.DbType.String },
                           { "@SortColumn", p => p.DbType = System.Data.DbType.String },
                           { "@SortDirection", p => p.DbType = System.Data.DbType.String },
                           { "@Severity", p => p.DbType = System.Data.DbType.String },
                           { "@ImpactRange", p => p.DbType = System.Data.DbType.AnsiString }
                       },
                       System.Data.CommandType.Text
                   );

            var response = new IssueTrackerResponse();

            var licensedModules = _licensingService.GetLicensedModuleString(organizationId, _configOptions);

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@UserGroupId", userGroupId},
                            { "@LicensedModules",  licensedModules},
                            { "@ScanGroupPermissionType", "ScanGroup" },
                            { "@ScanPermissionType", "Scan" },
                            { "@CheckpointPermissionType", "Checkpoint" },
                            { "@ScanId", String.IsNullOrWhiteSpace(request.ScanId) ? (object)DBNull.Value : Convert.ToInt32(request.ScanId) },
                            { "@ScanGroupId", String.IsNullOrWhiteSpace(request.ScanGroupId) ? (object)DBNull.Value : Convert.ToInt32(request.ScanGroupId) },
                            { "@RowsToFetch", String.IsNullOrWhiteSpace(request.RecordsToReturn)? (object)DBNull.Value : Convert.ToInt32(request.RecordsToReturn) },
                            { "@CurrentPage", String.IsNullOrWhiteSpace(request.CurrentPage)? (object)DBNull.Value : Convert.ToInt32(request.CurrentPage) },
                            { "@CheckpointGroupId", string.IsNullOrWhiteSpace(request.CheckpointGroupId) ? (object)DBNull.Value : request.CheckpointGroupId },
                            { "@CheckpointId", string.IsNullOrWhiteSpace(request.CheckpointId) ? (object)DBNull.Value : request.CheckpointId },
                            { "@State", request.State == null || request.State.Length == 0 ? (object)DBNull.Value : string.Join(",", request.State) },
                            { "@Page", string.IsNullOrWhiteSpace(request.Page) ? (object)DBNull.Value : request.Page },
                            { "@PriorityLevel", request.PriorityLevel == null || request.PriorityLevel.Length == 0  ? (object)DBNull.Value : string.Join(",", request.PriorityLevel) },
                            { "@SortColumn", string.IsNullOrWhiteSpace(request.SortColumn) ? (object)DBNull.Value : request.SortColumn },
                            { "@SortDirection", string.IsNullOrWhiteSpace(request.SortDirection) ? (object)DBNull.Value : request.SortDirection },
                            { "@Severity", request.Severity == null || request.Severity.Length == 0 ? (object)DBNull.Value : string.Join(",", request.Severity) },
                            { "@ImpactRange", request.ImpactRange == null || request.ImpactRange.Count == 0 ? "" : Helper.BuildImpactRange(request.ImpactRange) }
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        response.IssueTrackerList.Add(new IssueTrackerReportItem
                        {
                            Checkpoint = reader["Checkpoint"].ToString(),
                            Severity = reader["Severity"].ToString(),
                            HighestPageLevel = Convert.ToInt32(reader["HighestPageLevel"].ToString()),
                            Issue = reader["Issue"].ToString(),
                            Occurrences = Convert.ToInt32(reader["Occurrences"].ToString()),
                            Pages = Convert.ToInt32(reader["Pages"].ToString()),
                            Scans = Convert.ToInt32(reader["Scans"].ToString()),
                            State = reader["State"].ToString(),
                            Impact = reader["Impact"].ToString() == null ? 0 : Convert.ToDouble(reader["Impact"].ToString()),
                            PriorityLevel = Convert.ToInt32(reader["PriorityLevel"].ToString()),
                            CheckpointUrl = reader["CheckpointUrl"].ToString(),
                            CheckpointId = reader["CheckId"].ToString(),
                            IssueId = Convert.ToInt32(reader["IssueId"].ToString())
                        });
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        response.CheckpointList.Add(new CheckpointListItem
                        {
                            CheckpointId = reader["CheckpointId"].ToString(),
                            CheckpointDescription = reader["CheckpointDescription"].ToString()
                        });
                    }

                    await reader.NextResultAsync();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        response.TotalPagesScanned = Convert.ToInt32(reader["TotalPagesScanned"].ToString());
                        response.TotalIssuesFound = Convert.ToInt32(reader["TotalIssuesFound"].ToString());
                        response.TotalOccurrences = Convert.ToInt32(reader["TotalOccurrences"].ToString());
                        response.TotalPagesImpacted = Convert.ToInt32(reader["TotalPagesImpacted"].ToString());
                        response.TotalFilteredRecords = Convert.ToInt32(reader["TotalFilteredRecords"].ToString());
                        response.TotalFailedIssues = Convert.ToInt32(reader["TotalFailedIssues"].ToString());
                        response.TotalHighSeverityIssues = Convert.ToInt32(reader["TotalHighSeverityFailedIssues"].ToString());
                    }
                }
            }

            return response;
        }

        public async Task<IEnumerable<OccurrencesExportItem>> GetOccurrencesExport(int userGroupId, string organizationId, OccurrencesExportRequest request, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(occurrencesExportquery,
                                 new Dictionary<string, Action<DbParameter>>
                                 {
                                    { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@IssueId", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@CheckpointId", p => p.DbType = System.Data.DbType.String },
                                    { "@ScanGroupPermissionType", p => p.DbType = System.Data.DbType.String },
                                    { "@ScanPermissionType", p => p.DbType = System.Data.DbType.String },
                                    { "@LicensedModules", p => p.DbType = System.Data.DbType.String },
                                    { "@CheckpointGroupId", p => p.DbType = System.Data.DbType.String },
                                    { "@State", p => p.DbType = System.Data.DbType.String },
                                    { "@PageTitle", p => p.DbType = System.Data.DbType.String },
                                    { "@PageUrl", p => p.DbType = System.Data.DbType.String },
                                    { "@KeyAttributeNameValue", p => p.DbType = System.Data.DbType.String },
                                    { "@Element", p => p.DbType = System.Data.DbType.String },
                                    { "@ContainerId", p => p.DbType = System.Data.DbType.String },
                                    { "@ScanName", p => p.DbType = System.Data.DbType.String },
                                    { "@CurrentPage", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@RowsToFetch", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@SortColumn", p => p.DbType = System.Data.DbType.String },
                                    { "@SortDirection", p => p.DbType = System.Data.DbType.String }
                                 },
                                 System.Data.CommandType.Text
                             );

            var licensedModules = _licensingService.GetLicensedModuleString(organizationId, _configOptions);
            var occurrenceExportItems = new List<OccurrencesExportItem>();

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@UserGroupId", userGroupId},
                            { "@ScanGroupPermissionType", "ScanGroup" },
                            { "@ScanPermissionType", "Scan" },
                            { "@ScanId", string.IsNullOrWhiteSpace(request.ScanId) ? (object)DBNull.Value : Convert.ToInt32(request.ScanId) },
                            { "@ScanGroupId", string.IsNullOrWhiteSpace(request.ScanGroupId) ? (object)DBNull.Value : Convert.ToInt32(request.ScanGroupId) },
                            { "@IssueId", string.IsNullOrWhiteSpace(request.IssueId) ? (object)DBNull.Value : Convert.ToInt32(request.IssueId) },
                            { "@CheckpointId",  string.IsNullOrWhiteSpace(request.CheckpointId) ? (object)DBNull.Value: request.CheckpointId },
                            { "@LicensedModules",  licensedModules},
                            { "@CheckpointGroupId", string.IsNullOrWhiteSpace(request.CheckpointGroupId) ? (object)DBNull.Value : request.CheckpointGroupId },
                            { "@ScanName", string.IsNullOrWhiteSpace(request.ScanName) ? (object)DBNull.Value : request.ScanName },
                            { "@State", string.IsNullOrWhiteSpace(request.State) ? (object)DBNull.Value : request.State },
                            { "@PageTitle", request.PageTitle ?? (object)DBNull.Value },
                            { "@PageUrl", string.IsNullOrWhiteSpace(request.PageUrl) ? (object)DBNull.Value : request.PageUrl },
                            { "@KeyAttributeNameValue", request.KeyAttribute ?? (object)DBNull.Value },
                            { "@Element", request.Element ?? (object)DBNull.Value },
                            { "@ContainerId", request.ContainerId ?? (object)DBNull.Value },
                            { "@RowsToFetch", string.IsNullOrEmpty(request.RecordsToReturn) ? (object)DBNull.Value : request.RecordsToReturn },
                            { "@CurrentPage", string.IsNullOrEmpty(request.CurrentPage) ? (object)DBNull.Value : request.CurrentPage },
                            { "@SortColumn", string.IsNullOrWhiteSpace(request.SortColumn) ? (object)DBNull.Value : request.SortColumn },
                            { "@SortDirection", string.IsNullOrWhiteSpace(request.SortDirection) ? (object)DBNull.Value : request.SortDirection },
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        occurrenceExportItems.Add(new OccurrencesExportItem
                        {
                            PageTitle = reader["PageTitle"].ToString(),
                            PageUrl = reader["Url"].ToString(),
                            Element = reader["Element"].ToString(),
                            CheckpointGroupName = reader["CheckpointGroupName"].ToString(),
                            ScanDisplayName = reader["ScanDisplayName"].ToString(),
                            ScanId = Convert.ToInt32(reader["ScanId"].ToString()),
                            KeyAttributeName = reader["KeyAttrName"].ToString(),
                            KeyAttributeValue = reader["AttrValue"].ToString(),
                            Issue = reader["Issue"].ToString(),
                            ScanGroupName = reader["ScanGroupName"].ToString(),
                            ContainerId = reader["ContainerId"].ToString(),
                            CheckpointDescription = reader["CheckpointShortDescription"].ToString(),
                            CachedPageUrl = _urlService.SanitizeUrl(String.Format("{0}/ShowInstances.aspx?resultTextId={1}&runIds={2}&checkId={3}",
                                                                request.ReferrerUrl,
                                                                reader["ResultTextId"].ToString(),
                                                                reader["RunId"].ToString(),
                                                                reader["CheckpointId"].ToString()), request.ExportFormat == "xml")
                        });
                    }
                }
            }

            return occurrenceExportItems;
        }
    }
}
