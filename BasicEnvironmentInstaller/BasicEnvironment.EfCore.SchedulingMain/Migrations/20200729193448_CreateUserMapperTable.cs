using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicEnvironment.EfCore.SchedulingMain.Migrations
{
    public partial class CreateUserMapperTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"CREATE TABLE [dbo].[UserMapper](
	                        [UserMapperId] [int] IDENTITY(1,1) NOT NULL,
	                        [OrganizationId] [varchar](150) NOT NULL,
	                        [OrgUserId] [int] NULL,
	                        [CreateDate] [datetime] NULL,
	                        [LastModifiedDate] [datetime] NULL,
                         CONSTRAINT [PK_UserMapper] PRIMARY KEY CLUSTERED 
                        (
	                        [UserMapperId] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
                         CONSTRAINT [AK_UniqueOrgUser] UNIQUE NONCLUSTERED 
                        (
	                        [OrganizationId] ASC,
	                        [OrgUserId] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY]";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE [dbo].[UserMapper]");
        }
    }
}
