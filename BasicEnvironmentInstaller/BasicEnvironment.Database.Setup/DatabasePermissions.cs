using BasicEnvironment.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace BasicEnvironment.Database.Setup
{
    internal class DatabasePermissions
    {
        private readonly IClusterDataOptions dataOptions;
        private readonly ILogger logger;
        private readonly SqlConnection conn;
        private readonly string databaseName;
        private readonly string dbLoginName;
        private string dbUserName;

        public DatabasePermissions(IClusterDataOptions dataOptions, 
            ILogger logger, SqlConnection conn, string databaseName)
        {
            this.dataOptions = dataOptions;
            this.logger = logger;
            this.conn = conn;
            this.databaseName = databaseName;
            dbLoginName = dataOptions.ServiceAccountName.Replace(@"\\", @"\");
            dbUserName = dataOptions.ServiceAccountName.Replace(@"\\", @"\");
        }

        public void SetPermissionsForServiceAccount()
        {
            logger.LogInformation($"Applying User Permissions for {dbLoginName} on {databaseName}");
            CreateDatabaseUserIfNeeded();
            AddPermissions();
        }

        private void CreateDatabaseUserIfNeeded()
        {
            string userNameInDb = GetUserNameForLoginName();
            if (string.IsNullOrEmpty(userNameInDb))
            {
                string sql = $@"
                    USE [{databaseName}]
                    CREATE USER [{dbUserName}] FOR LOGIN [{dbLoginName}] WITH DEFAULT_SCHEMA=[dbo]";

                using (var createDatbaseCommand = new SqlCommand(sql, conn))
                {
                    createDatbaseCommand.ExecuteNonQuery();
                    logger.LogInformation($"User {dbUserName} for Login {dbLoginName} created on {databaseName}");
                }
            }
            else
            {
                logger.LogInformation($"User {dbUserName} for Login {dbLoginName} already exists on {databaseName}");
            }
        }

        private string GetUserNameForLoginName()
        {
            CheckForLoginInDb();
            return CheckUserForLogin();
        }

        private void CheckForLoginInDb()
        {
            logger.LogInformation($"Checking If Login {dbLoginName} exists in the sql server database host {dataOptions.DatabaseHost}");
            string sqlCheckForLogin = $@"Select name from master.sys.syslogins 
                                                where name='{dbLoginName}' and hasaccess=1 and denylogin = 0";

            logger.LogInformation($"Check for Login SQL Query: {sqlCheckForLogin}");

            using (var Command = new SqlCommand(sqlCheckForLogin, conn))
            {
                var reader = Command.ExecuteReader();
                if (!reader.HasRows)
                {
                    throw new Exception($"{dbLoginName} is not a valid login for the sql server database host {dataOptions.DatabaseHost}");
                }
                reader.Close();
            }
        }

        private string CheckUserForLogin()
        {
            logger.LogInformation($"Checking If User for Login {dbLoginName} exists in {databaseName}");
            string userNameInDB = string.Empty;
            string sqlUserForLogin = $@"SELECT 
                                UserId = u.uid, LoginName=l.loginname, UserName = u.name, LName=l.name 
                            FROM [{databaseName}].[sys].sysusers u 
                            join [{databaseName}].[sys].syslogins l on l.sid = u.sid 
                            where l.loginname=N'{dbLoginName}'";

            logger.LogInformation($"SQL Query for user: {sqlUserForLogin}");

            using (var Command = new SqlCommand(sqlUserForLogin, conn))
            {
                var reader = Command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var userId = reader[0].ToString();
                        var loginName = reader[1].ToString();
                        userNameInDB = reader[2].ToString();
                        dbUserName = userNameInDB;

                        logger.LogInformation($"UserId : {userId} | LoginName : {loginName} | UserName : {userNameInDB}");
                    }
                }
                reader.Close();
            }

            return userNameInDB;
        }

        private void AddPermissions()
        {
            string sql = $@" 
                USE [{databaseName}]
                EXEC sp_addrolemember 'db_datareader', N'{dbUserName}'
                EXEC sp_addrolemember 'db_datawriter', N'{dbUserName}'
                EXEC sp_addrolemember 'db_ddladmin', N'{dbUserName}'
                GRANT EXECUTE TO [{dbUserName}]
                GRANT CREATE VIEW TO [{dbUserName}]";

            using (var createDatbaseCommand = new SqlCommand(sql, conn))
            {
                createDatbaseCommand.ExecuteNonQuery();
                logger.LogInformation($"User {dbUserName} granted permissions on {databaseName}");
            }
        }
    }
}
