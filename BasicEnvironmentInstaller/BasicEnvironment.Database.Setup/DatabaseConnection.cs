using BasicEnvironment.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;

namespace BasicEnvironment.Database.Setup
{
    internal static class DatabaseConnection
    {
        internal static string GetSqlConnectionString(
            this IClusterDataOptions clusterOptions,
            string databaseName)
        {
            if (string.IsNullOrWhiteSpace(clusterOptions.DatabaseHost))
            {
                throw new ArgumentException("Invalid Database Credentials: You must provide a valid SQL Server instance name.", nameof(clusterOptions));
            }
            else if (!clusterOptions.UseWindowsAuthentication && (string.IsNullOrWhiteSpace(clusterOptions.AdminUserName) || string.IsNullOrWhiteSpace(clusterOptions.AdminPassword)))
            {
                throw new ArgumentException("Invalid Database Credentials: SQL Server authentication requires both a valid Username and a valid Password.", nameof(clusterOptions));
            }

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = clusterOptions.DatabaseHost,
                InitialCatalog = databaseName,
                // To Do: Should we make this configurable as well?
                ConnectTimeout = 5,
            };

            if (clusterOptions.UseWindowsAuthentication)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = clusterOptions.AdminUserName;
                builder.Password = clusterOptions.AdminPassword;
            }
            return builder.ConnectionString;
        }

        internal static void AssertValidSqlConnection(
            string connectionString,
            string databaseHost,
            ILogger logger
            )
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                logger.LogInformation($"Connection to SQL successful: {databaseHost}");
            }
        }

        internal static bool CheckIfDatabaseNeedsToBeCreated(SqlConnection conn, string databaseName, IClusterDataOptions cluster, ILogger logger)
        {
            using (var sqlCommand = new SqlCommand($"SELECT DB_ID ( N'{cluster.ClusterName}_{databaseName}' )", conn))
            {
                var returnValue = sqlCommand.ExecuteScalar();
                bool databaseMissing = String.IsNullOrEmpty(returnValue.ToString());
                logger.LogInformation($"Database '{cluster.ClusterName}_{databaseName}' needs to be created: {databaseMissing}");
                return databaseMissing;
            }
        }
    }
}
