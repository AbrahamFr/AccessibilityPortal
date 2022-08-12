USE [ComplianceSheriff_Farm004]
GO

/****** Object:  StoredProcedure [dbo].[GetTopCheckPointsByScanGroup]    Script Date: 8/20/2018 3:30:52 PM ******/
DROP PROCEDURE [dbo].[GetTopCheckPointsByScanGroup]
GO

/****** Object:  StoredProcedure [dbo].[GetTopCheckPointsByScanGroup]    Script Date: 8/20/2018 3:30:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO










CREATE Procedure [dbo].[GetTopCheckPointsByScanGroup]
    @ScanGroupId int,
	@StartDate datetime = NULL,
	@FinishDate datetime = NULL
AS
	SET NOCOUNT ON;

	IF @StartDate IS NULL
		SET @StartDate = DATEADD(year,-1,GETDATE())

	IF @FinishDate IS NULL
		SET @FinishDate = GETDATE()

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
	INSERT INTO @Reports
		-- Group, maybe has a parent and scans as descendants
		SELECT 'GROUP', Ancestor, AncestorParent, RunId
		FROM MiddleHierarchy
		INNER JOIN ScanGroupScans WITH (NOLOCK) ON ScanGroupScans.ScanGroupId=MiddleHierarchy.Descendant
		INNER JOIN MostRecentRunPerScan ON MostRecentRunPerScan.ScanId=ScanGroupScans.ScanId
		GROUP BY Ancestor, AncestorParent, RunId

		SELECT TOP 10 c.Number, c.ShortDescription, c.CheckpointId, 
        SUM(CASE WHEN Count=0 THEN 1 ELSE Count END) as Total 
        FROM
		(
			Select * FROM Results
			Where RunId IN (Select RunId FROM @Reports)
			  AND Result IN (0, 1, 2)
		) AS Results,
		Checkpoints c, Runs
        WHERE c.CheckpointId = Results.CheckId 
		AND Results.RunId = Runs.RunId
		GROUP BY c.ShortDescription, c.Number, c.CheckpointId  
		ORDER BY Total DESC
GO


