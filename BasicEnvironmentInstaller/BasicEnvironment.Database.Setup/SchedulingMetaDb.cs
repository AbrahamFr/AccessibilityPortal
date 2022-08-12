using System;
using System.Data.SqlClient;
using BasicEnvironment.Abstractions;
using Microsoft.Extensions.Logging;

namespace BasicEnvironment.Database.Setup
{
    internal class SchedulingMetaDb
    {
        private readonly IClusterDataOptions dataOptions;
        private readonly ILogger logger;
        private readonly SqlConnection conn;
        private readonly string databaseName;

        internal SchedulingMetaDb(IClusterDataOptions dataOptions, ILogger logger, SqlConnection conn)
        {
            this.dataOptions = dataOptions;
            this.logger = logger;
            this.conn = conn;
            this.databaseName = GetDatabaseName(dataOptions.ClusterName);
        }

        internal void PrepDatabase()
        {
            // 1. If the database is missing, create it and alter settings.
            CreateDatabaseIfMissing();
            new DatabasePermissions(dataOptions, logger, conn, databaseName).SetPermissionsForServiceAccount();
            // 2. If CS tables are present AND the EF Migration table is missing, we need to fake out EF. 
            //    Create the migration table and populate it with the Initial migration.
            CreateEfMigrationIfMissing();
        }

        public static string GetDatabaseName(string clusterName)
        {
            return $"{clusterName}_meta";
        }

        private void CreateDatabaseIfMissing()
        {
            logger.LogInformation($"Check for {databaseName} DataBase");

            // connect to database, query system table
            if (DatabaseConnection.CheckIfDatabaseNeedsToBeCreated(conn, "meta", dataOptions, logger))
            {
                string databaseLdfBackupDirectory = dataOptions.DatabaseLdfBackupDirectory ?? dataOptions.DatabaseMdfBackupDirectory;
                // if not exist, runs SQL script to create db
                var sql = $@"
                            CREATE DATABASE [{databaseName}]
	                            ON (
		                            NAME = N'{databaseName}_data',
		                            FILENAME = N'{dataOptions.DatabaseMdfBackupDirectory}\{databaseName}.mdf',
		                            FILEGROWTH = 10%)
	                             LOG ON (
		                            NAME = N'{databaseName}_log',
		                            FILENAME = N'{databaseLdfBackupDirectory}\{databaseName}.ldf',
		                            FILEGROWTH = 20%)
                            ";
                using (var createDatbaseCommand = new SqlCommand(sql, conn))
                {
                    createDatbaseCommand.ExecuteNonQuery();
                }

                logger.LogInformation($"{databaseName} DataBase was Created Successfully");
            }
        }



        private void CreateEfMigrationIfMissing()
        {
            // If it's an upgrade without EF, manually create EF Migrations table with Initial entry
            if (IsTablePresent($"{databaseName}.dbo.tasks") && !IsTablePresent($"{databaseName}.dbo.__EFMigrationsHistory"))
            {
                logger.LogInformation($"Detected upgrade. Database table '{databaseName}.dbo.__EFMigrationsHistory' needs to be created.");
                string[] dbSetupCommands = new string[] { $@"
USE [{databaseName}]
", $@"
SET ANSI_NULLS ON
", @"
SET QUOTED_IDENTIFIER ON
", @"
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
", @"
INSERT INTO [dbo].[__EFMigrationsHistory]
           ([MigrationId]
           ,[ProductVersion])
     VALUES
           ('20181121011047_Initial'
           ,'2.1.4-rtm-31024')
" };
                foreach (var sql in dbSetupCommands)
                {
                    using (var createDatbaseCommand = new SqlCommand(sql, conn))
                    {
                        createDatbaseCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        private bool IsTablePresent(string tableName)
        {
            // Connect to DB and check to see if existing Tables and such exist
            using (var sqlCommand = new SqlCommand($"SELECT OBJECT_ID ( N'{tableName}' )", conn))
            {
                var returnValue = sqlCommand.ExecuteScalar();
                return !String.IsNullOrEmpty(returnValue.ToString());
            }
        }
    }
}
