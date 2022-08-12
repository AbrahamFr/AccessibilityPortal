using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicEnvironment.EfCore.SchedulingMain.Migrations
{
    public partial class AddJobInitializeHistoricalDataForManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                DECLARE @DatabaseName varchar(100)

                IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
	                BEGIN
		                EXEC msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
	                END
                
                SET @DatabaseName = (Select DB_NAME())

                IF NOT EXISTS (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'PopulateScanGroupRuns')
	                BEGIN
		                EXEC  msdb.dbo.sp_add_job @job_name=N'PopulateScanGroupRuns', 
				                @enabled=1, 
				                @notify_level_eventlog=0, 
				                @notify_level_email=0, 
				                @notify_level_netsend=0, 
				                @notify_level_page=0, 
				                @delete_level=0, 
				                @description=N'Initially populates the reporting.ScanGroupRuns table for each Organizational Instance', 
				                @category_name=N'[Uncategorized (Local)]'
	                END

                    IF Not EXISTS(
	                    SELECT * FROM msdb.dbo.sysjobservers 
	                    WHERE 
		                    job_id in (SELECT job_id FROM msdb.dbo.sysjobs where [name] = 'PopulateScanGroupRuns')
		                    AND server_id = 0
                    )
                        EXEC msdb.dbo.sp_add_jobserver @job_name=N'PopulateScanGroupRuns', @server_name = N'(local)'

                    IF NOT EXISTS(SELECT b.step_name FROM [msdb].[dbo].[sysjobs] a WITH(NOLOCK) 
                            INNER JOIN [msdb].[dbo].[sysjobsteps] b WITH(NOLOCK) ON a.job_id = b.job_id 
                            WHERE a.Name = 'PopulateScanGroupRuns' and b.step_name = 'Initial Step' )
	                BEGIN
		                EXEC  msdb.dbo.sp_add_jobstep @job_name=N'PopulateScanGroupRuns', @step_name='Initial Step', 
				                @step_id=1, 
				                @cmdexec_success_code=0, 
				                @on_success_action=1, 
				                @on_success_step_id=0, 
				                @on_fail_action=2, 
				                @on_fail_step_id=0, 
				                @retry_attempts=0, 
				                @retry_interval=0, 
				                @os_run_priority=0, @subsystem=N'TSQL', 
				                @command=N'Select * FROM sys.syslogins', 
				                @database_name=@DatabaseName, 
				                @flags=0
	                END

            ";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'PopulateScanGroupRuns')
                                    EXEC sp_delete_job @job_name = N'PopulateScanGroupRuns';");
        }
    }
}
