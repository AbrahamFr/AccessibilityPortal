using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicEnvironment.EfCore.SchedulingMain.Migrations
{
    public partial class Intial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE [dbo].[ScheduledTasks](
	            [TaskName] [varchar](255) NOT NULL,
	            [ApplicationName] [varchar](255) NOT NULL,
	            [Parameters] [varchar](255) NOT NULL,
	            [WorkingDirectory] [varchar](255) NOT NULL,
	            [ProcessId] [int] NOT NULL,
	            [MachineName] [varchar](255) NULL,
	            [MostRecentRunTime] [datetime] NULL,
                PRIMARY KEY CLUSTERED 
                (
	                [TaskName] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]"
            );
            migrationBuilder.Sql(@"
                CREATE TABLE [dbo].[TaskTriggers](
	                [TriggerId] [int] IDENTITY(1,1) NOT NULL,
	                [TaskName] [varchar](255) NOT NULL,
	                [Period] [smallint] NOT NULL,
	                [PeriodCount] [smallint] NOT NULL,
	                [Week] [smallint] NOT NULL,
	                [WeekDay] [smallint] NOT NULL,
	                [StartTimeUtc] [datetime] NOT NULL,
                PRIMARY KEY CLUSTERED 
                (
	                [TriggerId] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]"
            );
            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[TaskTriggers]  WITH CHECK ADD FOREIGN KEY([TaskName])
                REFERENCES [dbo].[ScheduledTasks] ([TaskName])"
            );
            migrationBuilder.Sql(@"
                IF Not EXISTS (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'DoMaintenance')
                    EXEC msdb.dbo.sp_add_job @job_name='DoMaintenance'"
            );
            migrationBuilder.Sql(@"
                IF Not EXISTS (Select name from msdb.dbo.sysschedules where name = 'RegularMaintenance')
                    EXEC msdb.dbo.sp_add_schedule @schedule_name='RegularMaintenance', @freq_type=4, @freq_interval=1, @freq_subday_type=8, @freq_subday_interval=1, @active_start_time=200500, @active_end_time=080500
                EXEC msdb.dbo.sp_attach_schedule @job_name='DoMaintenance', @schedule_name='RegularMaintenance'
                IF NOT EXISTS(
                    SELECT * FROM msdb.dbo.sysjobservers 
                    WHERE 
                        job_id in (SELECT job_id FROM msdb.dbo.sysjobs where [name] = 'DoMaintenance')
                        AND server_id = 0
                    )
                        EXEC msdb.dbo.sp_add_jobserver @job_name='DoMaintenance'"
            );

            migrationBuilder.Sql(@"

                   IF NOT EXISTS(SELECT b.step_name FROM [msdb].[dbo].[sysjobs] a WITH(NOLOCK) 
                            INNER JOIN [msdb].[dbo].[sysjobsteps] b WITH(NOLOCK) ON a.job_id = b.job_id 
                            WHERE a.Name = 'DoMaintenance' and b.step_name = 'Initial Step' )
	                BEGIN

                        DECLARE @DatabaseName varchar(100)
                        SET @DatabaseName = (Select DB_NAME())

		                EXEC  msdb.dbo.sp_add_jobstep @job_name=N'DoMaintenance', @step_name='Initial Step', 
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
            ");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
