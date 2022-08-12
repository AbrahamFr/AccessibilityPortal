USE [ComplianceSheriff_Preview]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [idx_ReportQuery]    Script Date: 8/30/2018 8:38:58 AM ******/
CREATE NONCLUSTERED INDEX [idx_ReportQuery] ON [dbo].[Results]
(
	[RunId] ASC,
	[ResultId] ASC
)
INCLUDE ( 	[PageId],
	[CheckId],
	[Result],
	[Priority],
	[Count],
	[ResultTextId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


