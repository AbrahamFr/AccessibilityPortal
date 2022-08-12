USE [ComplianceSheriff_Release]
GO

/****** Object:  StoredProcedure [dbo].[GetAllScanGroupScanSummary_default]    Script Date: 8/27/2018 1:46:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO













ALTER Procedure [dbo].[GetAllScanGroupScanSummary_default]
    @ScanGroupId int = NULL,
	@StartDate datetime = NULL,
	@FinishDate datetime = NULL
AS
	SET NOCOUNT ON;

	IF @StartDate IS NULL
		SET @StartDate = DATEADD(YEAR,-1,GETDATE())

	IF @FinishDate IS NULL
		SET @FinishDate = GETDATE()

	DECLARE @MostRecentRunPerScan TABLE (ScanId int, RunId int)
	DECLARE @EarliestRuns TABLE (ScanId int, RunId int)

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
		GROUP BY s.ScanId)


	INSERT INTO @MostRecentRunPerScan
		Select * FROM MostRecentRunPerScan

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
	)

	INSERT INTO @EarliestRuns
	SELECT * FROM EarliestRunPerScan
	
	SELECT PageId, Checkid, RunId INTO #Results
		FROM Results WITH (NOLOCK)
	Where RunId IN (Select RunId FROM @MostRecentRunPerScan)
	AND Result = 0

	SELECT PageId, Checkid, RunId INTO #EarliestResults
		FROM Results WITH (NOLOCK)
	Where RunId IN (Select RunId FROM @EarliestRuns)
	AND Result = 0


		SELECT 
		ScanGroupsOuter.ScanGroupId,
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
				INNER JOIN ScanGroupScans WITH (NOLOCK)
					ON ScanGroupScans.ScanId = Runs.ScanId
				WHERE Pages.IsPage = 1
					AND Pages.Url IS NOT NULL
					AND Runs.RunId IN (Select RunId FROM @MostRecentRunPerScan)
					AND ScanGroupScans.ScanGroupId = ScanGroupsOuter.ScanGroupId
			) AS PageScans
		) AS PageScans,
		(
			-- Distinct Pages Scanned from Latest Runs
			SELECT COUNT(1)
			FROM
			(
				Select Distinct [Url] 
				FROM Pages WITH (NOLOCK)
				INNER JOIN Runs WITH (NOLOCK)
					ON Pages.RunId = Runs.RunId
				INNER JOIN ScanGroupScans WITH (NOLOCK)
					ON ScanGroupScans.ScanId = Runs.ScanId
				WHERE Pages.IsPage = 1
					AND Pages.Url IS NOT NULL
					AND Runs.RunId IN (Select RunId FROM @EarliestRuns)
					AND ScanGroupScans.ScanGroupId = ScanGroupsOuter.ScanGroupId
			) AS EarliestPageScans
		) AS EarliestPageScans,
		(
			SELECT COUNT(1)
			FROM
			(
				Select DISTINCT [Url] 
				FROM Pages WITH (NOLOCK)
				INNER JOIN #Results As ResultFailures
					 ON Pages.PageId = ResultFailures.PageId 
					AND Pages.RunId = ResultFailures.RunId
				INNER JOIN Runs WITH (NOLOCK)
					ON Pages.RunId = Runs.RunId
				INNER JOIN ScanGroupScans WITH (NOLOCK)
					ON ScanGroupScans.ScanId = Runs.ScanId
				WHERE Pages.IsPage = 1
					AND Pages.Url IS NOT NULL
					AND ScanGroupScans.ScanGroupId = ScanGroupsOuter.ScanGroupId
			) AS PageFailures
		) AS PageFailures,
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
				INNER JOIN ScanGroupScans WITH (NOLOCK)
					ON ScanGroupScans.ScanId = Runs.ScanId
				WHERE Pages.IsPage = 1
					AND Pages.Url IS NOT NULL
					AND ScanGroupScans.ScanGroupId = ScanGroupsOuter.ScanGroupId
			) AS EarliestPageFailures
		) AS EarliestPageFailures,
		(
			-- Latest Unique Checkpoints Checked
			SELECT COUNT(1)
			FROM
			(
				Select DISTINCT CheckId
				FROM Results WITH (NOLOCK)
				INNER JOIN Runs WITH (NOLOCK)
					ON Results.RunId = Runs.RunId
				INNER JOIN ScanGroupScans WITH (NOLOCK)
					ON ScanGroupScans.ScanId = Runs.ScanId
				WHERE Runs.RunId IN (Select RunId FROM @MostRecentRunPerScan)
					AND ScanGroupScans.ScanGroupId = ScanGroupsOuter.ScanGroupId
			) As CheckPointScans
		) As CheckPointScans,
		(
			-- Latest Unique Checkpoints Checked
			SELECT COUNT(1)
			FROM
			(
				Select DISTINCT CheckId
				FROM Results WITH (NOLOCK)
				INNER JOIN Runs WITH (NOLOCK)
					ON Results.RunId = Runs.RunId
				INNER JOIN ScanGroupScans WITH (NOLOCK)
					ON ScanGroupScans.ScanId = Runs.ScanId
				WHERE Runs.RunId IN (Select RunId FROM @EarliestRuns)
					AND ScanGroupScans.ScanGroupId = ScanGroupsOuter.ScanGroupId
			) As CheckPointScans
		) As EarliestCheckPointScans,
		(
			-- Distinct Checkpoint Failures
			SELECT COUNT(1)
			FROM
			(
				Select DISTINCT CheckId
				FROM #Results As ResultFailures
				INNER JOIN Runs WITH (NOLOCK)
					ON ResultFailures.RunId = Runs.RunId
				INNER JOIN ScanGroupScans WITH (NOLOCK)
					ON ScanGroupScans.ScanId = Runs.ScanId
				WHERE ResultFailures.RunId IN (Select RunId FROM @MostRecentRunPerScan)
					AND ScanGroupScans.ScanGroupId = ScanGroupsOuter.ScanGroupId
			) AS CheckPointFailures
		) AS CheckPointFailures,
		(
			-- Distinct Checkpoint Failures
			SELECT COUNT(1)
			FROM
			(
				Select DISTINCT CheckId
				FROM #EarliestResults As ResultFailures
				INNER JOIN Runs WITH (NOLOCK)
					ON ResultFailures.RunId = Runs.RunId
				INNER JOIN ScanGroupScans WITH (NOLOCK)
					ON ScanGroupScans.ScanId = Runs.ScanId
				WHERE ResultFailures.RunId IN (Select RunId FROM @EarliestRuns)
					AND ScanGroupScans.ScanGroupId = ScanGroupsOuter.ScanGroupId
			) AS EarliestCheckPointFailures
		) AS EarliestCheckPointFailures
	FROM 
	--(SELECT DISTINCT  ScanGroupId FROM @MostRecentRunPerScan As MRRPS
	--	LEFT OUTER JOIN ScanGroupScans WITH (NOLOCK)
	--		ON MRRPS.ScanId = ScanGroupScans.ScanId) As DistinctScanGroups
	--INNER JOIN 
		ScanGroups AS ScanGroupsOuter WITH (NOLOCK)
		--ON DistinctScanGroups.ScanGroupId = ScanGroupsOuter.ScanGroupId


	DROP TABLE #Results
	DROP TABLE #EarliestResults

GO


