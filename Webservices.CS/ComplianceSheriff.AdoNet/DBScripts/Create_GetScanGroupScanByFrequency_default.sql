USE [ComplianceSheriff_Farm004]
GO

/****** Object:  StoredProcedure [dbo].[GetScanGroupScanByFrequency_default]    Script Date: 8/20/2018 3:30:21 PM ******/
DROP PROCEDURE [dbo].[GetScanGroupScanByFrequency_default]
GO

/****** Object:  StoredProcedure [dbo].[GetScanGroupScanByFrequency_default]    Script Date: 8/20/2018 3:30:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[GetScanGroupScanByFrequency_default]
	@ScanGroupId int = NULL,
	@StartDate datetime = NULL,
	@FinishDate datetime = NULL, 
	@Frequency int = NULL
AS
	/*
		FREQUENCY DEFINITIONS:
	    DAY = 1
		WEEK = 2
		MONTH = 3
		YEAR = 4
	*/

	SET NOCOUNT ON;

	IF @ScanGroupId <= 0
		SET @ScanGroupId = NULL
	
	IF @Frequency IS NULL
		SET @Frequency = 1

	IF @StartDate IS NULL
		SET @StartDate = DATEADD(YEAR, -1, GETDATE())

	IF @FinishDate IS NULL
		SET @FinishDate = GETDATE()

	DECLARE @MostRecentRuns TABLE (ScanId int, RunId int)

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
	SELECT * FROM MostRecentRunPerScan

	SELECT PageId, RunId INTO #Results
	 FROM Results
	WHERE RunId IN (Select RunId FROM @MostRecentRuns)
	  AND Result = 0

	IF @Frequency = 1
		BEGIN
			SELECT	IndividualDate,
				(SELECT
					COUNT(Distinct [Url]) 
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND Pages.RunId IN (Select RunId FROM @MostRecentRuns)
						AND Runs.Finished BETWEEN IndividualDate AND Convert(DateTime, DATEDIFF(DAY, -1, IndividualDate))
			) As Scans, 
			(SELECT
					COUNT(DISTINCT [Url])
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					INNER JOIN #Results
					  ON #Results.RunId = Pages.RunId
					 AND #Results.PageId = Pages.PageId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND Finished BETWEEN IndividualDate AND Convert(DateTime, DATEDIFF(DAY, -1, IndividualDate))			  
				) As FailedPages 
			FROM DateRange('d', @StartDate, @FinishDate) As DateRange
		END

	-- BY WEEK
	IF @Frequency = 2
		BEGIN
			SELECT IndividualDate,
				(SELECT
					COUNT(Distinct [Url]) 
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND Pages.RunId IN (Select RunId FROM @MostRecentRuns)
						AND Finished BETWEEN IndividualDate AND DATEADD(dd, 7, IndividualDate)
				) As Scans,
				(SELECT 
					COUNT(DISTINCT [Url])
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					INNER JOIN #Results
					  ON #Results.RunId = Pages.RunId
					 AND #Results.PageId = Pages.PageId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND Finished BETWEEN IndividualDate AND DATEADD(dd, 7, IndividualDate)
				) As FailedPages
			FROM DateRange('w', @StartDate, @FinishDate) As DateRange
		END

	-- MONTH
	IF @Frequency = 3
		BEGIN
			SELECT IndividualDate,
				(SELECT 
					COUNT(Distinct [Url]) 
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND Pages.RunId IN (Select RunId FROM @MostRecentRuns)
						AND MONTH(Finished) = MONTH(IndividualDate)
					    AND YEAR(Finished) = YEAR(IndividualDate)
				) As Scans,
				(SELECT 
					COUNT(DISTINCT [Url]) 
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					INNER JOIN #Results
					  ON #Results.RunId = Pages.RunId
					 AND #Results.PageId = Pages.PageId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND MONTH(Finished) = MONTH(IndividualDate)
						AND YEAR(Finished) = YEAR(IndividualDate)
				) As FailedPages
			FROM DateRange('m', @StartDate, @FinishDate) As DateRange
		END

		-- YEAR
		IF @Frequency = 4
			BEGIN
				SELECT IndividualDate,
						(SELECT 
						COUNT(Distinct [Url]) 
						FROM Pages WITH (NOLOCK)
						INNER JOIN Runs
						  ON Pages.RunId = Runs.RunId
						WHERE Pages.IsPage = 1
							AND Pages.Url IS NOT NULL
							AND Pages.RunId IN (Select RunId FROM @MostRecentRuns)
							AND YEAR(Finished) = YEAR(IndividualDate)
						) As Scans,
						(SELECT 
							COUNT(DISTINCT [Url])
							FROM Pages WITH (NOLOCK)
							INNER JOIN Runs
							  ON Pages.RunId = Runs.RunId
							INNER JOIN #Results
							  ON #Results.RunId = Pages.RunId
							 AND #Results.PageId = Pages.PageId
							WHERE Pages.IsPage = 1
								AND Pages.Url IS NOT NULL
								AND YEAR(Finished) = YEAR(IndividualDate)
						) As FailedPages
				FROM DateRange('y', @StartDate, @FinishDate) As DateRange
			END

			DROP TABLE  #Results
GO


