USE [ComplianceSheriff_CS]
GO

/****** Object:  View [dbo].[PageRunResults]    Script Date: 7/26/2018 9:15:33 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[PageRunResults] AS
			SELECT Runs.RunId, Pages.PageId, Runs.ScanId,
				Runs.Started, Runs.Finished, Runs.AbortReason,
				Pages.Url, IsPage, Title, Pages.[Hash],
				Results.Result, Results.Priority, Results.CheckId, Runs.Status
			FROM dbo.Runs
			INNER JOIN dbo.Pages ON Runs.RunId=Pages.RunId AND Pages.IsPage=1
			INNER JOIN dbo.Results ON Pages.PageId=Results.PageId AND Pages.RunId=Results.RunId
			-- FIXME: Should use an index, but the field is varchar(MAX) on newer db instances and so cannot be indexed
			WHERE Runs.AbortReason IS NULL
GO


