using BasicEnvironment.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace BasicEnvironment.Setup.SharedDirectory
{
    [Description("File System Storage")]
    internal class FileSystemStorageSetup : ISetupStep
    {
        private readonly IClusterDataDirectoryOptions clusterOptions;
        private readonly ILogger<FileSystemStorageSetup> logger;
        private readonly SystemConfiguration configuration;
        private readonly NtAccounts ntAccounts;

        public FileSystemStorageSetup(
            IClusterDataDirectoryOptions clusterOptions,
            ILogger<FileSystemStorageSetup> logger,
            SystemConfiguration configuration,
            NtAccounts ntAccounts)
        {
            this.clusterOptions = clusterOptions;
            this.logger = logger;
            this.configuration = configuration;

            this.ntAccounts = ntAccounts;
        }

        public void Setup()
        {
            string sharedFullDataDirectory = clusterOptions.GetSharedFullDataDirectory(configuration);
            ManageDirectoryExistance(sharedFullDataDirectory);

            var shareableDir = Path.GetDirectoryName(sharedFullDataDirectory);
            logger.LogInformation($"shareableDir: {shareableDir}");
            CreateDistShareDirectory(sharedFullDataDirectory);

            var serviceAccountSecId = ntAccounts.ClusterServiceAccount;
            var adminsSecId = ntAccounts.GetAdminAccounts();

            SetPermissionsOnFolders(shareableDir, serviceAccountSecId, adminsSecId);

            logger.LogInformation($"Full Path: {sharedFullDataDirectory}");
        }

        private void CreateDistShareDirectory(string shareableDir)
        {
            var sharedDistDirectory = Path.Combine(shareableDir, "dist");
            Directory.CreateDirectory(sharedDistDirectory);
            var workingDirectory = Uri.UnescapeDataString(new Uri(Assembly.GetEntryAssembly().GetName().CodeBase).AbsolutePath);
            var sourceLocation = new FileInfo(workingDirectory).Directory.FullName;
            sourceLocation = Path.Combine(sourceLocation, "ProgramData");
            if (Directory.Exists(sourceLocation))
            {
                // Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourceLocation, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourceLocation, sharedDistDirectory));
                }

                // Copy all the files & Replaces any files with the same name
                foreach (string sourceFileLocation in Directory.GetFiles(sourceLocation, "*.*", SearchOption.AllDirectories))
                {
                    var destinationFileLocation = sourceFileLocation.Replace(sourceLocation, sharedDistDirectory);
                    File.Copy(sourceFileLocation, destinationFileLocation, true);
                }
            }
        }

        private void ManageDirectoryExistance(string sharedFullDataDirectory)
        {
            var dInfo = new DirectoryInfo(sharedFullDataDirectory);

            if (dInfo.Exists)
            {
                logger.LogInformation($"Shared Directory exists already: {sharedFullDataDirectory}");
            }
            else
            {
                dInfo.Create();
                logger.LogInformation($"Created Shared Directory: {sharedFullDataDirectory}");
            }
            ManageConfigurationToSql(sharedFullDataDirectory);
            var dInfoCustomers = new DirectoryInfo($"{sharedFullDataDirectory}\\customers");
            if (dInfoCustomers.Exists)
            {
                logger.LogInformation($"{sharedFullDataDirectory}\\customers exists already");
            }
            else
            {
                dInfoCustomers.Create();
                logger.LogInformation($"Created {sharedFullDataDirectory}\\customers");
            }
        }

        private void ManageConfigurationToSql(string sharedFullDataDirectory)
        {
            string configPath = $"{sharedFullDataDirectory}\\config\\sqlserver";
            var dInfoSql = new DirectoryInfo(configPath);
            if (dInfoSql.Exists)
            {
                logger.LogInformation($"{configPath} exists already");
            }
            else
            {
                dInfoSql.Create();
                logger.LogInformation($"Created {configPath}");
            }
        }

        #region FolderPermissions
        private void SetPermissionsOnFolders(string shareableDir, SecurityIdentifier serviceAccountSecId, SecurityIdentifier adminsSecId)
        {
            var dInfo = new DirectoryInfo(shareableDir);
            var dSecurity = dInfo.GetAccessControl();

            dSecurity.AddAccessRule(new FileSystemAccessRule(serviceAccountSecId, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
            dSecurity.AddAccessRule(new FileSystemAccessRule(adminsSecId, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));

            dInfo.SetAccessControl(dSecurity);
        }
        #endregion
    }
}
