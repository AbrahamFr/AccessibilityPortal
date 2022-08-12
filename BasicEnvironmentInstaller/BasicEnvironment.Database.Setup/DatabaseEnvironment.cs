using BasicEnvironment.EfCore.SchedulingMain;
using BasicEnvironment.EfCore.SchedulingMeta;
using BasicEnvironment.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;

namespace BasicEnvironment.Database.Setup
{
    [System.ComponentModel.Description("Databases and other SQL Scripts")]
    public class DatabaseEnvironment : ISetupStep
    {
        private readonly IClusterDataOptions dataOptions;
        private readonly ILogger<DatabaseEnvironment> logger;

        public DatabaseEnvironment(
            IClusterDataOptions dataOptions,
            ILogger<DatabaseEnvironment> logger
            )
        {
            this.dataOptions = dataOptions;
            this.logger = logger;
        }

        public void Setup()
        {
            var connectionString = dataOptions.GetSqlConnectionString("master");

            PrepSchedulingDbs(connectionString);

            // run EF
            var connectionStringMain = dataOptions.GetSqlConnectionString(SchedulingMainDb.GetDatabaseName(dataOptions.ClusterName));
            UpgradeDbContext(() => new SchedulingMainDbContext(SchedulingMainDbContext.CreateConfiguration(connectionStringMain)));

            var connectionStringMeta = dataOptions.GetSqlConnectionString(SchedulingMetaDb.GetDatabaseName(dataOptions.ClusterName));
            UpgradeDbContext(() => new SchedulingMetaDbContext(SchedulingMetaDbContext.CreateConfiguration(connectionStringMeta)));

            SqlServerAgentOwnershipChange(connectionString);

            logger.LogInformation("Databases setup complete.");
        }

        private void UpgradeDbContext(Func<DbContext> dbContextFactory)
        {
            using (var dbContext = dbContextFactory())
            {
                dbContext.Database.Migrate();
            }
        }

        private void PrepSchedulingDbs(string connectionString)
        {

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                new SchedulingMainDb(dataOptions, logger, conn).PrepDatabase();
                new SchedulingMetaDb(dataOptions, logger, conn).PrepDatabase();
            }
        }
        private void SqlServerAgentOwnershipChange(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                new SqlAgentManager(dataOptions, logger, conn).ChangeOwnerToServiceAccount();
            }
        }
    }
}
