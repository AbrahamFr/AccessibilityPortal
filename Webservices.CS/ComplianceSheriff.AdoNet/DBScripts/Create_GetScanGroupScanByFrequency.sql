USE [ComplianceSheriff_Farm004]
GO

/****** Object:  StoredProcedure [dbo].[GetScanGroupScanByFrequency]    Script Date: 8/20/2018 3:29:49 PM ******/
DROP PROCEDURE [dbo].[GetScanGroupScanByFrequency]
GO

/****** Object:  StoredProcedure [dbo].[GetScanGroupScanByFrequency]    Script Date: 8/20/2018 3:29:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO














CREATE PROCEDURE [dbo].[GetScanGroupScanByFrequency]
	@ScanGroupId int,
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
	
	IF @Frequency IS NULL
		SET @Frequency = 1

	IF @StartDate IS NULL
		SET @StartDate = DATEADD(MONTH, -3, GETDATE())

	IF @FinishDate IS NULL
		SET @FinishDate = GETDATE()

	--SELECT @StartDate
	--SELECT @FinishDate

	DECLARE @Reports TABLE (
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
			WHERE (Ancestor=@ScanGroupId)
		UNION
			SELECT @ScanGroupId
	), MiddleHierarchy AS (
			SELECT NULL AS AncestorParent, @ScanGroupId AS Ancestor, @ScanGroupId AS Descendant
		UNION ALL
			SELECT NULL AS AncestorParent, Ancestor, Descendant
			FROM ScanGroupHierarchy
			WHERE (Ancestor=@ScanGroupId)
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
	INSERT INTO @Reports
		-- Group, maybe has a parent and scans as descendants
		SELECT 'GROUP', Ancestor, AncestorParent, RunId
		FROM MiddleHierarchy
		INNER JOIN ScanGroupScans WITH (NOLOCK) ON ScanGroupScans.ScanGroupId=MiddleHierarchy.Descendant
		INNER JOIN MostRecentRunPerScan ON MostRecentRunPerScan.ScanId=ScanGroupScans.ScanId
		GROUP BY Ancestor, AncestorParent, RunId
	UNION ALL
		-- Scan, definitely has a parent group.
		SELECT 'SCAN', TargetScans.ScanId, TargetScans.ScanGroupId, MostRecentRunPerScan.RunId
		FROM TargetScans
		INNER JOIN MostRecentRunPerScan ON MostRecentRunPerScan.ScanId=TargetScans.ScanId
		GROUP BY TargetScans.ScanId, TargetScans.ScanGroupId, MostRecentRunPerScan.RunId
	UNION ALL
		-- Run, may or may not be used in the Scan results, but is present for comparing history within time period.
		SELECT 'RUN', RunId, Runs.ScanId, RunId
		FROM Runs WITH (NOLOCK)
		INNER JOIN TargetScans ON Runs.ScanId=TargetScans.ScanId
		WHERE Finished Between @StartDate AND @FinishDate
		GROUP BY RunId, Runs.ScanId, RunId

	--SELECT * FROM @Reports

	SELECT * INTO #Results
	  FROM Results
	WHERE Results.RunId IN (Select RunId FROM @Reports)

	IF @Frequency = 1
		BEGIN
			SELECT	IndividualDate,
				(SELECT
					COUNT(DISTINCT [Url])
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					INNER JOIN #Results AS Results 
						ON Results.RunId = Pages.RunId
					   AND Results.PageId = Pages.PageId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
					    AND CONVERT(Date, Runs.Finished) = IndividualDate
						AND Result = 3
			) As Scans, 
			(SELECT
					COUNT(DISTINCT [Url])
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					INNER JOIN #Results AS Results 
						ON Results.RunId = Pages.RunId
					   AND Results.PageId = Pages.PageId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
					    AND CONVERT(Date, Runs.Finished) = IndividualDate
						AND  Result = 0					  
				) As FailedPages 
			FROM DateRange('d', @StartDate, @FinishDate) As DateRange
		END

	-- BY WEEK
	IF @Frequency = 2
		BEGIN
			SELECT IndividualDate,
				(SELECT 
					COUNT(DISTINCT [Url])
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					INNER JOIN #Results AS Results 
						ON Results.RunId = Pages.RunId
					   AND Results.PageId = Pages.PageId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND Finished BETWEEN IndividualDate AND DATEADD(dd, 7, IndividualDate)
					    AND Result = 3
				) As Scans,
				(SELECT 
					COUNT(DISTINCT [Url])
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					INNER JOIN #Results AS Results 
						ON Results.RunId = Pages.RunId
					   AND Results.PageId = Pages.PageId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND Finished BETWEEN IndividualDate AND DATEADD(dd, 7, IndividualDate)
					  AND Result = 0
			) As FailedPages
			FROM DateRange('w', @StartDate, @FinishDate) As DateRange
		END

	-- MONTH
	IF @Frequency = 3
		BEGIN
			SELECT IndividualDate,
				(SELECT 
					COUNT(DISTINCT [Url])
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					INNER JOIN #Results AS Results 
						ON Results.RunId = Pages.RunId
					   AND Results.PageId = Pages.PageId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
						AND MONTH(Finished) = MONTH(IndividualDate)
					    AND YEAR(Finished) = YEAR(IndividualDate)
					  AND Result = 3
				) As Scans,
				(SELECT 
					COUNT(DISTINCT [Url]) AS PassedPages
					FROM Pages WITH (NOLOCK)
					INNER JOIN Runs
					  ON Pages.RunId = Runs.RunId
					INNER JOIN #Results AS Results 
						ON Results.RunId = Pages.RunId
					   AND Results.PageId = Pages.PageId
					WHERE Pages.IsPage = 1
						AND Pages.Url IS NOT NULL
					    AND MONTH(Finished) = MONTH(IndividualDate)
					    AND YEAR(Finished) = YEAR(IndividualDate)
					    AND Result = 0
			) As FailedPages
			FROM DateRange('m', @StartDate, @FinishDate) As DateRange
		END

		-- YEAR
		IF @Frequency = 4
			BEGIN
				SELECT IndividualDate,
						(SELECT 
							COUNT(DISTINCT [Url])
							FROM Pages WITH (NOLOCK)
							INNER JOIN Runs
							  ON Pages.RunId = Runs.RunId
							INNER JOIN #Results AS Results 
								ON Results.RunId = Pages.RunId
							   AND Results.PageId = Pages.PageId
							WHERE Pages.IsPage = 1
								AND Pages.Url IS NOT NULL
							    AND YEAR(Finished) = YEAR(IndividualDate)
							    AND Result = 3
						) As Scans,
						(SELECT 
							COUNT(DISTINCT [Url])
							FROM Pages WITH (NOLOCK)
							INNER JOIN Runs
							  ON Pages.RunId = Runs.RunId
							INNER JOIN #Results AS Results 
								ON Results.RunId = Pages.RunId
							   AND Results.PageId = Pages.PageId
							WHERE Pages.IsPage = 1
								AND Pages.Url IS NOT NULL
							    AND YEAR(Finished) = YEAR(IndividualDate)
							    AND Result = 0
						) As FailedPages
				FROM DateRange('y', @StartDate, @FinishDate) As DateRange
			END
GO


