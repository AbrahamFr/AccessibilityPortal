USE [<DatabaseName>]
GO

/****** Object:  UserDefinedFunction [dbo].[DateRange]    Script Date: 8/17/2018 1:21:00 PM ******/
DROP FUNCTION [dbo].[DateRange]
GO

/****** Object:  UserDefinedFunction [dbo].[DateRange]    Script Date: 8/17/2018 1:21:00 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE FUNCTION [dbo].[DateRange]
(     
      @Increment              CHAR(1),
      @StartDate              DATETIME,
      @EndDate                DATETIME
)
RETURNS  
@SelectedRange    TABLE 
(IndividualDate DATETIME)
AS 
BEGIN
      ;WITH cteRange (DateRange) AS (
            SELECT @StartDate
            UNION ALL
            SELECT 
                  CASE
                        WHEN @Increment = 'd' THEN DATEADD(dd, 1, DateRange)
                        WHEN @Increment = 'w' THEN DATEADD(ww, 1, DateRange)
                        WHEN @Increment = 'm' THEN DATEADD(mm, 1, DateRange)
						WHEN @Increment = 'y' THEN DATEADD(yyyy, 1, DateRange)
                  END
            FROM cteRange
            WHERE DateRange <= 
                  CASE
                        WHEN @Increment = 'd' THEN DATEADD(dd, -1, @EndDate)
                        WHEN @Increment = 'w' THEN DATEADD(ww, -1, @EndDate)
                        WHEN @Increment = 'm' THEN DATEADD(mm, -1, @EndDate)
						WHEN @Increment = 'y' THEN DATEADD(yyyy, -1, @EndDate)
                  END)
          
      INSERT INTO @SelectedRange (IndividualDate)
      SELECT dateadd(day, +0, convert(varchar, DateRange, 101))
      FROM cteRange
      OPTION (MAXRECURSION 3660);
      RETURN
END
GO


