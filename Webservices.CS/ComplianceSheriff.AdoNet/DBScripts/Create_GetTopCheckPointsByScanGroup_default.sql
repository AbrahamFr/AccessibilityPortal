USE [ComplianceSheriff_Farm004]
GO

/****** Object:  StoredProcedure [dbo].[GetTopCheckPointsByScanGroup_default]    Script Date: 8/20/2018 3:31:25 PM ******/
DROP PROCEDURE [dbo].[GetTopCheckPointsByScanGroup_default]
GO

/****** Object:  StoredProcedure [dbo].[GetTopCheckPointsByScanGroup_default]    Script Date: 8/20/2018 3:31:25 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



















CREATE Procedure [dbo].[GetTopCheckPointsByScanGroup_default]
	@ScanGroupId int = NULL,
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

		SELECT TOP 10 c.Number, c.ShortDescription, c.CheckpointId, 
        SUM(CASE WHEN Count=0 THEN 1 ELSE Count END) as Total
        FROM
		(
			Select * FROM Results
			Where RunId IN (Select RunId FROM MostRecentRunPerScan)
			  AND Result IN (0, 1, 2)
		) AS Results,
		Checkpoints c, Runs
        WHERE c.CheckpointId = Results.CheckId 
		AND Results.RunId = Runs.RunId
		GROUP BY c.ShortDescription, c.Number, c.CheckpointId  
		ORDER BY Total DESC
GO


