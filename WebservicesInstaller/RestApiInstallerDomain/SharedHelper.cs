using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using ComplianceSheriff.Domain.Installers.Abstractions;
using ComplianceSheriff.Domain.Installers.Extensions;

namespace RestApiInstallerDomain
{
    public static class SharedHelper
    {    
        public static string GetDBaseServerName(IUrnOptions urnOptions, SystemConfiguration configuration)
        {
            string dbServerName = string.Empty;
            string sharedFullDataDirectory = $@"\\{urnOptions.GetSharedFullDataDirectory(configuration)}";
            string sharedDataFilePath = Path.Combine(sharedFullDataDirectory, "config", "sqlserver", "default.xml");
            var xml = XDocument.Load(sharedDataFilePath);

            var query = from c in xml.Root.Descendants("Host")
                        select c;

            dbServerName = query.FirstOrDefault().Value;
            return dbServerName;
        }

        public static string BuildDBConnectionString(string databaseName, IAllOptions options, IUrnOptions urnOptions, SystemConfiguration configuration)
        {
            string connectionString = string.Empty;

            string dbServerName = GetDBaseServerName(urnOptions, configuration);

            if (databaseName.First().Equals('_'))
            {
                databaseName = $"{options.ClusterName}{databaseName}";
            }

            if (options.UseWindowsAuthentication)
            {
                connectionString = String.Format("Server={0};Database={1};Trusted_Connection=True", dbServerName, databaseName);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(options.DataBaseAdminName) && !string.IsNullOrWhiteSpace(options.DataBaseAdminPassword))
                {
                    connectionString = String.Format("Server={0};Database={1};User Id={2};Password={3}", dbServerName.ToUpper(), databaseName, options.DataBaseAdminName, options.DataBaseAdminPassword);
                }
                else
                {
                    throw new ArgumentException("Invalid Database Credentials: SQL Server authentication requires both a valid Username and a valid Password.");
                }
            }

            return connectionString;
        }

        public static string BuildFullDestinationPath(IAllOptions allOptions, IFolderOptions folderOptions, SystemConfiguration configuration)
        {
            var fullPath = folderOptions.GetFullDestinationDirectory(configuration);
            var fullDestinationPath = Path.Combine(fullPath,
                                          fullPath.ToLower().Substring(fullPath.LastIndexOf('\\')).IndexOf("webservices") == -1 ? "webServices" : "",
                                          fullPath.ToLower().Substring(fullPath.LastIndexOf('\\')).IndexOf(allOptions.TargetAppName.ToLower()) == -1
                                          ? allOptions.TargetAppName : "");
            return fullDestinationPath;
        }
    }
}
