using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicEnvironment.EfCore.SchedulingMain.Migrations
{
    public partial class InsertQueueServerBaseUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"INSERT INTO dbo.ClusterSettings VALUES('QueueServerBaseUrl', 'http://localhost:9000/')";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE dbo.ClusterSettings Where SettingsKey = 'QueueServerBaseUrl'");
        }
    }
}
