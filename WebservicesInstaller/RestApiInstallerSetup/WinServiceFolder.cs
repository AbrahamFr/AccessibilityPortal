using ComplianceSheriff.Domain.Installers.Abstractions;
using ComplianceSheriff.Domain.Installers.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;

namespace RestApiInstallerSetup
{
    public class WinServiceFolder : ISetupStep
    {
        private readonly IClusterSettings clusterSettings;
        private readonly IFolderOptions folderOptions;
        private readonly IAccountOptions accountOptions;
        private readonly ILogger<WinServiceFolder> logger;
        private readonly IOptions<SystemConfiguration> configuration;
        private readonly QueueServiceConfigEditor queueServiceConfigEditor;
        private readonly IFileSecurityManager fileSecurityManager;
        private readonly IFileSystemManager fileSystemManager;
        private readonly ISystemAccountManager systemAccountManager;
        private string fullDestinationDirectory;

        public WinServiceFolder(
            IFolderOptions folderOptions,
            IAccountOptions accountOptions,
            ILogger<WinServiceFolder> logger,
            QueueServiceConfigEditor queueServiceConfigEditor,
            IClusterSettings clusterSettings,
            IFileSecurityManager fileSecurityManager,
            IFileSystemManager fileSystemManager,
            ISystemAccountManager systemAccountManager,
            IOptions<SystemConfiguration> configuration)
        {
            this.folderOptions = folderOptions;
            this.accountOptions = accountOptions;
            this.logger = logger;
            this.queueServiceConfigEditor = queueServiceConfigEditor;
            this.configuration = configuration;
            this.clusterSettings = clusterSettings;
            this.systemAccountManager = systemAccountManager;
            this.fileSecurityManager = fileSecurityManager;
            this.fileSystemManager = fileSystemManager;
        }

        public void Setup()
        {
            fullDestinationDirectory = Path.Combine(folderOptions.GetFullDestinationDirectory(configuration.Value), "queueService");

            if (Directory.Exists(fullDestinationDirectory))
            {
                if (fileSystemManager.HasContent(fullDestinationDirectory))
                {
                    fileSystemManager.RemoveContent(fullDestinationDirectory);
                }
            } else
            {
                fileSystemManager.CreateFolder(fullDestinationDirectory);
            }

            var serviceAccountSecId = systemAccountManager.GetNtAccount(accountOptions.ServiceAccountName);
            var adminsSecId = systemAccountManager.GetAdminAccounts();
            fileSecurityManager.SetPermissions(serviceAccountSecId, adminsSecId, fullDestinationDirectory);
 
            fileSystemManager.DeployFiles(folderOptions.QueueServiceSourceDirectory, fullDestinationDirectory);
      
            queueServiceConfigEditor.SetQueueServerConfiguration(fullDestinationDirectory);
        }
    }
}
