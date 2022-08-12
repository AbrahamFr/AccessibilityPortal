USE [ComplianceSheriff_Release]
GO

/****** Object:  StoredProcedure [dbo].[GetScanGroupMetrics_default]    Script Date: 8/27/2018 1:47:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO














CREATE Procedure [dbo].[GetScanGroupMetrics_default]
    @ScanGroupId int = NULL,
	@StartDate datetime = NULL,
	@FinishDate datetime = NULL
AS
	SET NOCOUNT ON;

	IF @StartDate IS NULL
		SET @StartDate = DATEADD(YEAR,-1,GETDATE())

	IF @FinishDate IS NULL
		SET @FinishDate = GETDATE()

	DECLARE @MostRecentRuns TABLE (RunId int)
	DECLARE @EarliestRuns TABLE (RunId int)


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
	)

	INSERT INTO @MostRecentRuns
	SELECT RunId FROM MostRecentRunPerScan

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
	SELECT RunId FROM EarliestRunPerScan

	SELECT  NULL As ScanGroupId,
			'All Scan Groups' As DisplayName,
	(
		-- Distinct Pages Scanned from Latest Runs
		SELECT COUNT(1)
		FROM
		(
			Select Distinct [Url] 
			FROM Pages WITH (NOLOCK)
			WHERE Pages.IsPage = 1
				AND Pages.Url IS NOT NULL
				AND Pages.RunId IN (Select RunId FROM @MostRecentRuns)
		) AS LatestPagesScanned
	) AS LatestPagesScanned,
	(
		-- Distinct Pages Scanned from Earliest Runs
		SELECT COUNT(1)
		FROM
		(
			Select Distinct [Url] 
			FROM Pages WITH (NOLOCK)
			WHERE Pages.IsPage = 1
				AND Pages.Url IS NOT NULL
				AND Pages.RunId IN (Select RunId FROM @EarliestRuns)
		) AS EarliestPagesScanned
	) AS EarliestPagesScanned,
	(
		-- DISTINCT PAGE Failures
		SELECT COUNT(1)
		FROM
		(
			Select DISTINCT [Url] 
			FROM Pages WITH (NOLOCK)
			INNER JOIN Results WITH (NOLOCK)
				ON Pages.PageId = Results.PageId 
				AND Pages.RunId = Results.RunId
			WHERE Pages.IsPage = 1
				AND Pages.Url IS NOT NULL
				AND Result = 0
				AND Pages.RunId IN (Select RunId FROM @MostRecentRuns)
		) AS LatestPageFailures
	) AS LatestPageFailures,
	(
		-- DISTINCT PAGE Failures
		SELECT COUNT(1)
		FROM
		(
			Select DISTINCT [Url] 
			FROM Pages WITH (NOLOCK)
			INNER JOIN Results WITH (NOLOCK)
				ON Pages.PageId = Results.PageId 
				AND Pages.RunId = Results.RunId
			WHERE Pages.IsPage = 1
				AND Pages.Url IS NOT NULL
				AND Result = 0
				AND Pages.RunId IN (Select RunId FROM @EarliestRuns)
		) AS EarliestPageFailures
	) AS EarliestPageFailures,
	(
		-- Latest Unique Checkpoints Checked
		SELECT COUNT(1)
		FROM
		(
			Select DISTINCT CheckId
			FROM Results WITH (NOLOCK)
			WHERE Results.RunId IN (Select RunId FROM @MostRecentRuns)
		) As LatestCheckPointsChecked
	) AS LatestCheckPointsChecked,
	(
	-- Unique Checkpoints Checked
	SELECT COUNT(1)
	FROM
	(
			Select DISTINCT CheckId
			FROM Results WITH (NOLOCK)
			WHERE Results.RunId IN (Select RunId FROM @EarliestRuns)
		) As EarliestCheckPointsChecked
	) AS EarliestCheckPointsChecked,
	(
		-- Distinct Checkpoint Failures
		SELECT COUNT(1)
		FROM
		(	
			Select DISTINCT CheckId
			FROM Results WITH (NOLOCK)
			WHERE Results.RunId IN (Select RunId FROM @MostRecentRuns)
				AND Result = 0
		) AS LatestCheckPointFailures
	) AS LatestCheckPointFailures,
	(
		-- Distinct Checkpoint Failures
		SELECT COUNT(1)
		FROM
		(	
			Select DISTINCT CheckId
			FROM Results WITH (NOLOCK)
			WHERE Results.RunId IN (Select RunId FROM @EarliestRuns)
				AND Result = 0
		) AS EarliestCheckPointFailures
	) AS EarliestCheckPointFailures

GO


