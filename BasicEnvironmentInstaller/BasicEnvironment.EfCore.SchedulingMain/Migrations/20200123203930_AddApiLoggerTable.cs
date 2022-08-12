using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicEnvironment.EfCore.SchedulingMain.Migrations
{
    public partial class AddApiLoggerTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE TABLE [dbo].[APIAuditLogs](
	                        [UserName] [varchar](250) NULL,
	                        [APIEndpoint] [varchar](1000) NULL,
	                        [Method] [varchar](25) NULL,
	                        [Organization] [varchar](1000) NULL,
	                        [Parameters] [varchar](max) NULL,
	                        [UserAgent] [varchar](5000) NULL,
	                        [RequestTime] [datetime] NOT NULL,
	                        [JsonWebToken] [varchar](max) NULL
                        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                        GO

                        ALTER TABLE [dbo].[APIAuditLogs] ADD  CONSTRAINT [DF_APIAuditLogs_RequestTime]  DEFAULT (getdate()) FOR [RequestTime]
                        GO

                        CREATE TABLE [dbo].[APIAuditLogsReporting](
	                        [Id] [int] IDENTITY(1,1) NOT NULL,
	                        [UserName] [varchar](250) NULL,
	                        [APIEndpoint] [varchar](1000) NULL,
	                        [Method] [varchar](25) NULL,
	                        [Organization] [varchar](1000) NULL,
	                        [Parameters] [varchar](max) NULL,
	                        [UserAgent] [varchar](5000) NULL,
	                        [RequestTime] [datetime] NOT NULL,
	                        [JsonWebToken] [varchar](max) NULL,
                         CONSTRAINT [PK_APIAuditLogsReporting] PRIMARY KEY CLUSTERED 
                        (
	                        [Id] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                        GO

                        CREATE PROCEDURE APIAuditLogManager
                        AS

                        SELECT * INTO #TmpAPIAuditLogs FROM dbo.APIAuditLogs
                        Where DATEDIFF(dd, RequestTime, getDate()) < 7

                        DELETE #TmpAPIAuditLogs
                        WHERE CHARINDEX('.', REVERSE(APIEndpoint)) BETWEEN 1 AND 5

                        MERGE dbo.APIAuditLogsReporting r
                        USING #TmpAPIAuditLogs d
                          ON r.UserName = d.UserName
                         AND r.APIEndpoint = d.APIEndpoint
                         AND r.Organization = d.Organization
                         AND r.Parameters = d.Parameters
                         AND r.UserAgent = d.UserAgent
                         AND r.RequestTime = d.RequestTime

                        WHEN NOT MATCHED BY TARGET 
                         THEN 
                           INSERT (UserName,APIEndpoint, Method, Organization, Parameters, UserAgent, RequestTime, JsonWebToken)
                           VALUES (d.UserName, d.APIEndpoint, d.Method, d.Organization, d.Parameters, d.UserAgent, d.RequestTime, d.JsonWebToken);

                        DROP TABLE #TmpAPIAuditLogs


                        --Clean up dbo.APIAuditLog table 
                        DELETE dbo.APIAuditLogs
                        WHERE DATEDIFF(dd, RequestTime, getDate()) > 30
                          AND CHARINDEX('.', REVERSE(APIEndpoint)) BETWEEN 1 AND 5

                        DELETE dbo.APIAuditLogs
                        WHERE DATEDIFF(dd, RequestTime, getDate()) > 60";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE [dbo].[APIAuditLogs]
                                   DROP TABLE [dbo].[APIAuditLogsReporting]
                                   DROP PROCEDURE APIAuditLogManager");
        }
    }
}
