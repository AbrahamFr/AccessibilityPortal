using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Data.SqlClient;
//using System.ServiceProcess;
using BasicEnvironment.Abstractions;

namespace BasicEnvironment.Database.Setup
{
    [System.ComponentModel.Description("SQL Services: verify enabled and running")]
    public class SqlServicesChecker : ISetupStep
    {
        private readonly ILogger<SqlServicesChecker> logger;
        private readonly IClusterDataOptions _dataOptions;

        public SqlServicesChecker(ILogger<SqlServicesChecker> logger, IClusterDataOptions dataOptions)
        {
            this.logger = logger;
            this._dataOptions = dataOptions;
        }
        public void Setup()
        {
            var connectionString = _dataOptions.GetSqlConnectionString("master");

            DatabaseConnection.AssertValidSqlConnection(
                            connectionString,
                            _dataOptions.DatabaseHost,
                            logger
                            );

            logger.LogInformation("Checking Sql Server and its components are installed and services running.");

            CheckSqlAgentServiceStatus(connectionString);
        }

        private void CheckSqlAgentServiceStatus(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var sql = @"Select status, status_desc FROM sys.dm_server_services
                            WHERE servicename LIKE '%SQL Server Agent%'";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var status = Convert.ToInt32(reader[0].ToString());
                                var statusDesc = reader[1].ToString();

                                if (status == 4 && statusDesc.ToUpper() == "RUNNING")
                                {
                                    logger.LogInformation("Sql Server Agent is installed and running.");
                                }
                                else
                                {
                                    throw new InvalidOperationException("Sql Server Agent Needs to be running for installation.");
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Sql Server and its components needs to be installed or enabled.");
                        }
                    }
                }
            }
        }
    }
}
