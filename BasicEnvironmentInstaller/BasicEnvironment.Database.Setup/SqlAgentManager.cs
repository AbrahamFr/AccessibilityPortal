using System;
using System.Data.SqlClient;
using BasicEnvironment.Abstractions;
using Microsoft.Extensions.Logging;

namespace BasicEnvironment.Database.Setup
{
    internal class SqlAgentManager
    {
        private readonly IClusterDataOptions dataOptions;
        private readonly ILogger logger;
        private readonly SqlConnection conn;

        internal SqlAgentManager(IClusterDataOptions dataOptions, ILogger logger, SqlConnection conn)
        {
            this.dataOptions = dataOptions;
            this.logger = logger;
            this.conn = conn;
        }

        internal void ChangeOwnerToServiceAccount()
        {
            ChangeOnwershipForAgentJob("DoMaintenance");
            ChangeOnwershipForAgentJob("PopulateScanGroupRuns");
        }

        private void ChangeOnwershipForAgentJob(string jobName)
        {
            logger.LogInformation($"Changing ownership of {jobName} to {dataOptions.ServiceAccountName} ");
            var sql = $@" 
EXEC msdb.dbo.sp_update_job
      @job_name = N'{jobName}',
      @owner_login_name = '{dataOptions.ServiceAccountName}'
";
            using (var createDatbaseCommand = new SqlCommand(sql, conn))
            {
                createDatbaseCommand.ExecuteNonQuery();
            }

            logger.LogInformation($"Ownership of {jobName} changed to {dataOptions.ServiceAccountName}!");
        }
    }
}
