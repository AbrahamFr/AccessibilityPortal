using BasicEnvironment.Abstractions;
using CommandLine;
using System;

namespace BasicEnvironment.Setup
{
    public class Options : IClusterAllOptions
    {
        [Option('q', "quiet", Required = false, HelpText = "Run without prompts to the user.")]
        public bool Quiet { get; set; }

        [Option("clusterName", Required = false, Default = "ComplianceSheriff", HelpText = "Distinquish server farms or clusters.")]
        public string ClusterName { get; set; }

        [Option("useWindowsAuthentication", Required = false, HelpText = "Connect to SQL via Windows Authentication during installation.")]
        public bool UseWindowsAuthentication { get; set; }
        [Option("databaseAdminName", Required = false, HelpText = "Name for the account to setup the Databases.")]
        public string DatabaseAdminName { get; set; }
        [Option("databaseAdminPassword", Required = false, HelpText = "Password for the account to setup the Databases.")]
        public string DatabaseAdminPassword { get; set; }


        [Option("databaseServerInstanceName", Required = true, HelpText = @"Name for Database Server and Instance.  (Instance name is optional, if not present.)  Example is 'MyServer\MyInstance'  or 'MyServer' .")]
        public string DatabaseServerInstanceName { get; set; }
        [Option("databaseMdfBackupDirectory", Required = true, HelpText = @"Path to the MDF Directory used for Backup of SQL Server.  Example is 'E:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA' .")]
        public string DatabaseMdfBackupDirectory { get; set; }
        [Option("databaseLdfBackupDirectory", Required = false, HelpText = @"Path to the LDF Directory used for Backup of SQL Server.  Example is 'E:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA' .")]
        public string DatabaseLdfBackupDirectory { get; set; }

        [Option("sharedDataDirectory", Required = true, HelpText = @"Inform where the Shared Directory for the Cluster goes.  Example is 'C:\ProgamData\{CompanyName}\ComplianceSheriff\' .")]
        public string SharedDataDirectory { get; set; }

        [Option("serviceAccountName", Required = true, HelpText = "Name for the account that the Compliance Sheriff account will run under.")]
        public string ServiceAccountName { get; set; }

        string IClusterDataOptions.DatabaseHost => DatabaseServerInstanceName;

        public string ShareableFolderName { get; set; }

        string IClusterDataOptions.AdminUserName => DatabaseAdminName;

        string IClusterDataOptions.AdminPassword => DatabaseAdminPassword;

    }
}
