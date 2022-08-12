USE [<DatabaseName>]
GO

/****** Object:  Index [IDX_ScanId_ScanGroupId]    Script Date: 8/17/2018 1:19:07 PM ******/
DROP INDEX [IDX_ScanId_ScanGroupId] ON [dbo].[ScanGroupScans]
GO

/****** Object:  Index [IDX_ScanId_ScanGroupId]    Script Date: 8/17/2018 1:19:07 PM ******/
CREATE NONCLUSTERED INDEX [IDX_ScanId_ScanGroupId] ON [dbo].[ScanGroupScans]
(
	[ScanGroupId] ASC,
	[ScanId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


