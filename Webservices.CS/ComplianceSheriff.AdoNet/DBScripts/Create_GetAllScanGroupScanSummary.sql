USE [ComplianceSheriff_Release]
GO

/****** Object:  StoredProcedure [dbo].[GetAllScanGroupScanSummary]    Script Date: 8/28/2018 10:40:07 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO










CREATE Procedure [dbo].[GetAllScanGroupScanSummary]
    @ScanGroupId int = NULL,
	@StartDate datetime = NULL,
	@FinishDate datetime = NULL
AS
	SET NOCOUNT ON;

	IF @StartDate IS NULL
		SET @StartDate = DATEADD(YEAR,-1,GETDATE())

	IF @FinishDate IS NULL
		SET @FinishDate = GETDATE()

	DECLARE @MostRecentRunsReport TABLE (
		ReportType varchar(255) NOT NULL,
		-- This target id doesn't mean anything by itself, but with the ReportType, it tells what object it is based on
		-- If ReportType=GROUP, then ReportTargetId=ScanGroupId
		-- If ReportType=SCAN, then ReportTargetId=ScanId
		-- If ReportType=RUN, then ReportTargetId=RunId, and ScanId will be the scan id
		ReportTargetId int NOT NULL,
		ParentScanGroupId int,
		RunId int NOT NULL
		--Ancestor int NULL,
		--AncestorParent int NOT NULL
	);

	DECLARE @EarliestRunsReport TABLE (
		ReportType varchar(255) NOT NULL,
		-- This target id doesn't mean anything by itself, but with the ReportType, it tells what object it is based on
		-- If ReportType=GROUP, then ReportTargetId=ScanGroupId
		-- If ReportType=SCAN, then ReportTargetId=ScanId
		-- If ReportType=RUN, then ReportTargetId=RunId, and ScanId will be the scan id
		ReportTargetId int NOT NULL,
		ParentScanGroupId int,
		RunId int NOT NULL
	);

 	;WITH MostRecentRunPerScan AS (
		SELECT s.ScanId, MAX(RunId) AS RunId
		FROM (
		  -- FIXME: Remove the CONVERT if the Runs table ever gets an int (as is correct) for the ScanId type
		  SELECT CONVERT(INT, ScanId) AS ScanId, MAX(Finished) AS Finished, MIN(Started) AS Started
		  FROM Runs WITH (NOLOCK)
		  WHERE Finished Between @StartDate AND @FinishDate
			-- FIXME: Should use an index, but the field is varchar(MAX) on newer db instances and so cannot be indexed
			AND Runs.AbortReason IS NULL
		  GROUP BY ScanId
		) AS s
		INNER JOIN Runs WITH (NOLOCK) ON s.ScanId=Runs.ScanId AND s.Finished=Runs.Finished
		-- this next line prevents us from finding Runs that were created before the Scan was. 
		-- FIXME: remove this comment and the next line when ScanIds no longer get reused.
		INNER JOIN Scans WITH (NOLOCK) ON Scans.ScanId=s.ScanId AND s.Started >= Scans.DateCreated
		GROUP BY s.ScanId
	), ScanGroupHierarchy AS (
		-- Limit of 100 children deep by default in SQL Server, it is configurable (go look it up, you need to do some work if you're changing it)
			SELECT ScanGroupId As Ancestor, SubgroupId As Descendant
			FROM ScanGroupSubgroups WITH (NOLOCK)
		UNION ALL
			SELECT Ancestor, SubgroupId As Descendant
			FROM ScanGroupHierarchy
			INNER JOIN ScanGroupSubgroups WITH (NOLOCK) ON Descendant=ScanGroupId
	), Parents AS (
			SELECT targets.Descendant AS Parent
			FROM ScanGroupHierarchy AS targets WITH (NOLOCK)
			WHERE Ancestor=@ScanGroupId
		UNION
			SELECT @ScanGroupId
	), MiddleHierarchy AS (
			SELECT NULL AS AncestorParent, @ScanGroupId AS Ancestor, @ScanGroupId AS Descendant
		UNION ALL
			SELECT NULL AS AncestorParent, Ancestor, Descendant
			FROM ScanGroupHierarchy
			WHERE Ancestor=@ScanGroupId
		UNION ALL
			SELECT Parents.Parent, ScanGroupSubgroups.SubgroupId, Descendant
			FROM Parents
			INNER JOIN ScanGroupSubgroups WITH (NOLOCK) ON ScanGroupSubgroups.ScanGroupId=Parent
			INNER JOIN ScanGroupHierarchy ON Ancestor=ScanGroupSubgroups.SubgroupId
		UNION ALL
			SELECT Parents.Parent, ScanGroupSubgroups.SubgroupId, ScanGroupSubgroups.SubgroupId
			FROM Parents
			INNER JOIN ScanGroupSubgroups WITH (NOLOCK) ON ScanGroupSubgroups.ScanGroupId=Parent
	)
	INSERT INTO @MostRecentRunsReport
		-- Group, maybe has a parent and scans as descendants
		SELECT 'GROUP', Ancestor, AncestorParent, RunId
		FROM MiddleHierarchy
		INNER JOIN ScanGroupScans WITH (NOLOCK) ON ScanGroupScans.ScanGroupId=MiddleHierarchy.Descendant
		INNER JOIN MostRecentRunPerScan ON MostRecentRunPerScan.ScanId=ScanGroupScans.ScanId
		GROUP BY Ancestor, AncestorParent, RunId

	--SELECT * FROM @MostRecentRunsReport

	SELECT PageId, Checkid, MostRecentRunsReport.RunId, ReportTargetId INTO #MostRecentRunsResults
		FROM Results WITH (NOLOCK)
	LEFT OUTER JOIN @MostRecentRunsReport AS MostRecentRunsReport
	  ON MostRecentRunsReport.RunId = Results.RunId
	Where Results.RunId IN (Select RunId FROM @MostRecentRunsReport)
	AND Result = 0

	--SELECT * FROM #MostRecentRunsResults
	--SELECT * FROM @MostRecentRunsReport


	;WITH EarliestRunPerScan AS (
		SELECT s.ScanId, MIN(RunId) AS RunId
		FROM (
		  -- FIXME: Remove the CONVERT if the Runs table ever gets an int (as is correct) for the ScanId type
		  SELECT CONVERT(INT, ScanId) AS ScanId, MIN(Finished) AS Finished, MIN(Started) AS Started
		  FROM Runs WITH (NOLOCK)
		  WHERE Finished Between @StartDate AND @FinishDate
			-- FIXME: Should use an index, but the field is varchar(MAX) on newer db instances and so cannot be indexed
			AND Runs.AbortReason IS NULL
		  GROUP BY ScanId
		) AS s
		INNER JOIN Runs WITH (NOLOCK) ON s.ScanId=Runs.ScanId AND s.Finished=Runs.Finished
		-- this next line prevents us from finding Runs that were created before the Scan was. 
		-- FIXME: remove this comment and the next line when ScanIds no longer get reused.
		INNER JOIN Scans WITH (NOLOCK) ON Scans.ScanId=s.ScanId AND s.Started >= Scans.DateCreated
		GROUP BY s.ScanId
	), ScanGroupHierarchy AS (
		-- Limit of 100 children deep by default in SQL Server, it is configurable (go look it up, you need to do some work if you're changing it)
			SELECT ScanGroupId As Ancestor, SubgroupId As Descendant
			FROM ScanGroupSubgroups WITH (NOLOCK)
		UNION ALL
			SELECT Ancestor, SubgroupId As Descendant
			FROM ScanGroupHierarchy
			INNER JOIN ScanGroupSubgroups WITH (NOLOCK) ON Descendant=ScanGroupId
	), Parents AS (
			SELECT targets.Descendant AS Parent
			FROM ScanGroupHierarchy AS targets WITH (NOLOCK)
			WHERE Ancestor=@ScanGroupId
		UNION
			SELECT @ScanGroupId
	), MiddleHierarchy AS (
			SELECT NULL AS AncestorParent, @ScanGroupId AS Ancestor, @ScanGroupId AS Descendant
		UNION ALL
			SELECT NULL AS AncestorParent, Ancestor, Descendant
			FROM ScanGroupHierarchy
			WHERE Ancestor=@ScanGroupId
		UNION ALL
			SELECT Parents.Parent, ScanGroupSubgroups.SubgroupId, Descendant
			FROM Parents
			INNER JOIN ScanGroupSubgroups WITH (NOLOCK) ON ScanGroupSubgroups.ScanGroupId=Parent
			INNER JOIN ScanGroupHierarchy ON Ancestor=ScanGroupSubgroups.SubgroupId
		UNION ALL
			SELECT Parents.Parent, ScanGroupSubgroups.SubgroupId, ScanGroupSubgroups.SubgroupId
			FROM Parents
			INNER JOIN ScanGroupSubgroups WITH (NOLOCK) ON ScanGroupSubgroups.ScanGroupId=Parent
	), TargetScans AS (
		SELECT [ScanGroupScans].ScanGroupId, [ScanGroupScans].ScanId
		FROM MiddleHierarchy
		INNER JOIN [ScanGroupScans] WITH (NOLOCK) ON [ScanGroupScans].ScanGroupId=MiddleHierarchy.Descendant
	)
	INSERT INTO @EarliestRunsReport
		-- Group, maybe has a parent and scans as descendants
		SELECT 'GROUP', Ancestor, AncestorParent, RunId
		FROM MiddleHierarchy
		INNER JOIN ScanGroupScans WITH (NOLOCK) ON ScanGroupScans.ScanGroupId=MiddleHierarchy.Descendant
		INNER JOIN EarliestRunPerScan ON EarliestRunPerScan.ScanId=ScanGroupScans.ScanId
		GROUP BY Ancestor, AncestorParent, RunId

	SELECT PageId, CheckId, RunId INTO #EarliestResults
		FROM Results WITH (NOLOCK)
	Where RunId IN (Select RunId FROM @EarliestRunsReport)
	AND Result = 0

	--SELECT * FROM @MostRecentRunsReport
	--SELECT * FROM @EarliestRunsReport

	--SELECT * FROM #MostRecentRunsResults
	--SELECT * FROM #EarliestResults

		SELECT 
		ScanGroupId,
		DisplayName,
		(
			-- Distinct Pages Scanned from Latest Runs
			SELECT COUNT(1)
			FROM
			(
				Select Distinct [Url] 
				FROM Pages WITH (NOLOCK)
				INNER JOIN Runs WITH (NOLOCK)
					ON Pages.RunId = Runs.RunId
				INNER JOIN @MostRecentRunsReport AS TargetRuns 
					ON TargetRuns.RunId = Runs.RunId
				WHERE Pages.IsPage = 1
					AND Pages.Url IS NOT NULL
					AND TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
			) AS PageScans
		) AS PageScans,
		(
			SELECT COUNT(1)
			FROM
			(
					Select DISTINCT [Url] 
					FROM Pages WITH (NOLOCK)
					INNER JOIN #MostRecentRunsResults As ResultFailures
						 ON Pages.PageId = ResultFailures.PageId 
						AND Pages.RunId = ResultFailures.RunId
					INNER JOIN Runs WITH (NOLOCK)
						ON Pages.RunId = Runs.RunId
					INNER JOIN @MostRecentRunsReport AS TargetRuns 
						ON TargetRuns.RunId = Runs.RunId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
				) AS PageFailures
			) AS PageFailures,
			(
			-- Latest Unique Checkpoints Checked
			SELECT COUNT(1)
			FROM
			(
					Select DISTINCT CheckId
					FROM Results WITH (NOLOCK)
					INNER JOIN Runs WITH (NOLOCK)
						ON Results.RunId = Runs.RunId
					INNER JOIN @MostRecentRunsReport AS TargetRuns 
						ON TargetRuns.RunId = Runs.RunId
					WHERE TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
				) As CheckPointScans
			) As CheckPointScans,
			(
			-- Distinct Checkpoint Failures
			SELECT COUNT(1)
			FROM
			(
				Select DISTINCT CheckId
				FROM #MostRecentRunsResults As ResultFailures
				INNER JOIN Runs WITH (NOLOCK)
					ON ResultFailures.RunId = Runs.RunId
				INNER JOIN @MostRecentRunsReport AS TargetRuns 
					ON TargetRuns.RunId = Runs.RunId
				WHERE TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
			) AS CheckPointFailures
		) AS CheckPointFailures,
		(
			-- Distinct Pages Scanned from Earliest Runs
			SELECT COUNT(1)
			FROM
			(
				Select Distinct [Url] 
				FROM Pages WITH (NOLOCK)
				INNER JOIN Runs WITH (NOLOCK)
					ON Pages.RunId = Runs.RunId
				INNER JOIN @EarliestRunsReport AS TargetRuns 
					ON TargetRuns.RunId = Runs.RunId
				WHERE Pages.IsPage = 1
					AND Pages.Url IS NOT NULL
					AND TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
			) AS EarliestPageScans
		) AS EarliestPageScans,
		(
			SELECT COUNT(1)
			FROM
			(
					Select DISTINCT [Url] 
					FROM Pages WITH (NOLOCK)
					INNER JOIN #EarliestResults As ResultFailures
						 ON Pages.PageId = ResultFailures.PageId 
						AND Pages.RunId = ResultFailures.RunId
					INNER JOIN Runs WITH (NOLOCK)
						ON Pages.RunId = Runs.RunId
					INNER JOIN @EarliestRunsReport AS TargetRuns 
						ON TargetRuns.RunId = Runs.RunId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
				) AS EarliestPageFailures
			) AS EarliestPageFailures,
			(
			-- Earliest Unique Checkpoints Checked
			SELECT COUNT(1)
			FROM
			(
					Select DISTINCT CheckId
					FROM Results WITH (NOLOCK)
					INNER JOIN Runs WITH (NOLOCK)
						ON Results.RunId = Runs.RunId
					INNER JOIN @EarliestRunsReport AS TargetRuns 
						ON TargetRuns.RunId = Runs.RunId
					WHERE TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
				) As EarliestCheckPointScans
			) As EarliestCheckPointScans,
			(
			-- Distinct Checkpoint Failures
			SELECT COUNT(1)
			FROM
			(
				Select DISTINCT CheckId
				FROM #EarliestResults As ResultFailures
				INNER JOIN Runs WITH (NOLOCK)
					ON ResultFailures.RunId = Runs.RunId
				INNER JOIN @EarliestRunsReport AS TargetRuns 
					ON TargetRuns.RunId = Runs.RunId
				WHERE TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
			) AS EarliestCheckPointFailures
		) AS EarliestCheckPointFailures
	FROM 
		ScanGroups
	WHERE ScanGroupID IN (Select DISTINCT ReportTargetId FROM @MostRecentRunsReport Where ParentScangroupId = @ScanGroupId)

	UNION

		SELECT 
		ScanGroups.ScanGroupId,
		DisplayName,
		(
			-- Distinct Pages Scanned from Latest Runs
			SELECT COUNT(1)
			FROM
			(
				Select Distinct [Url] 
				FROM Pages WITH (NOLOCK)
				INNER JOIN Runs WITH (NOLOCK)
					ON Pages.RunId = Runs.RunId
				INNER JOIN @MostRecentRunsReport AS TargetRuns 
					ON TargetRuns.RunId = Runs.RunId
				WHERE Pages.IsPage = 1
					AND Pages.Url IS NOT NULL
					AND TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
			) AS PageScans
		) AS PageScans,
		(
			SELECT COUNT(1)
			FROM
			(
					Select DISTINCT [Url] 
					FROM Pages WITH (NOLOCK)
					INNER JOIN #MostRecentRunsResults As ResultFailures
						 ON Pages.PageId = ResultFailures.PageId 
						AND Pages.RunId = ResultFailures.RunId
					INNER JOIN Runs WITH (NOLOCK)
						ON Pages.RunId = Runs.RunId
					INNER JOIN @MostRecentRunsReport AS TargetRuns 
						ON TargetRuns.RunId = Runs.RunId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
				) AS PageFailures
			) AS PageFailures,
			(
			-- Latest Unique Checkpoints Checked
			SELECT COUNT(1)
			FROM
			(
					Select DISTINCT CheckId
					FROM Results WITH (NOLOCK)
					INNER JOIN Runs WITH (NOLOCK)
						ON Results.RunId = Runs.RunId
					INNER JOIN @MostRecentRunsReport AS TargetRuns 
						ON TargetRuns.RunId = Runs.RunId
					WHERE TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
				) As CheckPointScans
			) As CheckPointScans,
			(
			-- Distinct Checkpoint Failures
			SELECT COUNT(1)
			FROM
			(
				Select DISTINCT CheckId
				FROM #MostRecentRunsResults As ResultFailures
				INNER JOIN Runs WITH (NOLOCK)
					ON ResultFailures.RunId = Runs.RunId
				INNER JOIN @MostRecentRunsReport AS TargetRuns 
					ON TargetRuns.RunId = Runs.RunId
				WHERE TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
			) AS CheckPointFailures
		) AS CheckPointFailures,
		(
			-- Distinct Pages Scanned from Earliest Runs
			SELECT COUNT(1)
			FROM
			(
				Select Distinct [Url] 
				FROM Pages WITH (NOLOCK)
				INNER JOIN Runs WITH (NOLOCK)
					ON Pages.RunId = Runs.RunId
				INNER JOIN @EarliestRunsReport AS TargetRuns 
					ON TargetRuns.RunId = Runs.RunId
				WHERE Pages.IsPage = 1
					AND Pages.Url IS NOT NULL
					AND TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
			) AS EarliestPageScans
		) AS EarliestPageScans,
		(
			SELECT COUNT(1)
			FROM
			(
					Select DISTINCT [Url] 
					FROM Pages WITH (NOLOCK)
					INNER JOIN #EarliestResults As ResultFailures
						 ON Pages.PageId = ResultFailures.PageId 
						AND Pages.RunId = ResultFailures.RunId
					INNER JOIN Runs WITH (NOLOCK)
						ON Pages.RunId = Runs.RunId
					INNER JOIN @EarliestRunsReport AS TargetRuns 
						ON TargetRuns.RunId = Runs.RunId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
				) AS EarliestPageFailures
			) AS EarliestPageFailures,
			(
			-- Earliest Unique Checkpoints Checked
			SELECT COUNT(1)
			FROM
			(
					Select DISTINCT CheckId
					FROM Results WITH (NOLOCK)
					INNER JOIN Runs WITH (NOLOCK)
						ON Results.RunId = Runs.RunId
					INNER JOIN @EarliestRunsReport AS TargetRuns 
						ON TargetRuns.RunId = Runs.RunId
					WHERE TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
				) As EarliestCheckPointScans
			) As EarliestCheckPointScans,
			(
			-- Distinct Checkpoint Failures
			SELECT COUNT(1)
			FROM
			(
				Select DISTINCT CheckId
				FROM #EarliestResults As ResultFailures
				INNER JOIN Runs WITH (NOLOCK)
					ON ResultFailures.RunId = Runs.RunId
				INNER JOIN @EarliestRunsReport AS TargetRuns 
					ON TargetRuns.RunId = Runs.RunId
				WHERE TargetRuns.ReportTargetId = ScanGroups.ScanGroupId
			) AS EarliestCheckPointFailures
		) AS EarliestCheckPointFailures
	FROM 
		ScanGroups WITH (NOLOCK)
	INNER JOIN ScanGroupScans WITH (NOLOCK)
	   ON ScanGroups.ScanGroupId = ScanGroupScans.ScanGroupId
	WHERE ScanGroups.ScanGroupId = @ScanGroupId

   DROP TABLE #MostRecentRunsResults
   DROP TABLE #EarliestResults

GO


