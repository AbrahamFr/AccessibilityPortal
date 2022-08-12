using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicEnvironment.EfCore.SchedulingMain.Migrations
{
    public partial class AddAPILoggerManagerJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                    DECLARE @JobName varchar(100), @DatabaseName varchar(100), @ServerName varchar(255),  @OwnerName varchar(255)

                    SET @DatabaseName = (Select DB_NAME())
                    SET @JobName = LEFT(@DatabaseName, CHARINDEX('_', @DatabaseName)-1) + '_APIAuditLoggerManager'
                    SET @OwnerName = (SELECT SYSTEM_USER)

                    IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
                    BEGIN
	                    EXEC msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
                    END

                    DECLARE @jobId BINARY(16)
                    EXEC  msdb.dbo.sp_add_job @job_name=@JobName, 
		                    @enabled=1, 
		                    @notify_level_eventlog=0, 
		                    @notify_level_email=0, 
		                    @notify_level_netsend=0, 
		                    @notify_level_page=0, 
		                    @delete_level=0, 
		                    @description=N'Manages data from APIAuditLogs to APIAuditLogsReporting in the <ClusterName>_main database.', 
		                    @category_name=N'[Uncategorized (Local)]',
		                    @owner_login_name=@OwnerName, @job_id = @jobId OUTPUT

                    EXEC msdb.dbo.sp_add_jobstep @job_name=@JobName, @step_name=N'Run APIAuditLogManager Procedure', 
		                    @step_id=1, 
		                    @cmdexec_success_code=0, 
		                    @on_success_action=1, 
		                    @on_success_step_id=0, 
		                    @on_fail_action=2, 
		                    @on_fail_step_id=0, 
		                    @retry_attempts=0, 
		                    @retry_interval=0, 
		                    @os_run_priority=0, @subsystem=N'TSQL', 
		                    @command=N'EXEC APIAuditLogManager', 
		                    @database_name=@DatabaseName, 
		                    @flags=0

                    EXEC msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
                    EXEC msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                DECLARE @JobName varchar(100), @DatabaseName varchar(100)

                SET @DatabaseName = (Select DB_NAME())
                SET @JobName = LEFT(@DatabaseName, CHARINDEX('_', @DatabaseName)-1) + '_APIAuditLoggerManager'

                EXEC msdb.dbo.sp_delete_job @job_name = @JobName";

            migrationBuilder.Sql(sql);
        }
    }
}
