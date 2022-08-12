USE [ComplianceSheriff_Farm004]
GO

/****** Object:  StoredProcedure [dbo].[GetTopIssuesByScanGroup_default]    Script Date: 8/20/2018 3:32:11 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





















CREATE Procedure [dbo].[GetTopIssuesByScanGroup_default]
    @ScanGroupId int = null,
	@StartDate datetime = NULL,
	@FinishDate datetime = NULL
AS
	SET NOCOUNT ON;

	IF @StartDate IS NULL
		SET @StartDate = DATEADD(YEAR,-1,GETDATE())

	IF @FinishDate IS NULL
		SET @FinishDate = GETDATE()



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

	SELECT TOP 10 ResultTexts.Value AS ResultText, Result As ResultStatus, Priority AS CheckPointPriority, COUNT(ResultTextId) as Total,
                            CheckId, Min(ResultTextId) AS ResultTextId
	FROM 
		(SELECT ResultTextId, Result, Priority, CheckId FROM RESULTS 
		  WHERE Results.RunId IN (SELECT RunId FROM MostRecentRunPerScan)
		    AND Result IN (0, 1, 2)
		  ) AS PagesWithResultTextIds
		INNER JOIN ResultTexts WITH (NOLOCK)
			ON PagesWithResultTextIds.ResultTextId = ResultTexts.ValueId
	GROUP BY ResultTexts.Value, ResultTextId, Result, Priority, CheckId
	ORDER BY Result, Priority, Total DESC
GO


