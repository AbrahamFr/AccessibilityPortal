using ServiceInstaller.Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ComponentModel;
using Microsoft.Extensions.Options;

namespace ServiceInstaller
{
    [Description("Folder for Program files Setup")]
    internal class ProgramFolder : ISetupStep
    {
        private readonly IFolderOptions folderOptions;
        private readonly IAccountOptions accountOptions;
        private readonly ILogger<ProgramFolder> logger;
        private readonly SystemConfiguration configuration;
        private string fullDestinationDirectory;

        public ProgramFolder(
            IFolderOptions folderOptions,
            IAccountOptions accountOptions,
            ILogger<ProgramFolder> logger,
            IOptions<SystemConfiguration> configuration)
        {
            this.folderOptions = folderOptions;
            this.accountOptions = accountOptions;
            this.logger = logger;
            this.configuration = configuration.Value;
        }
        public void Setup()
        {
            fullDestinationDirectory = folderOptions.GetFullDestinationDirectory(configuration);
            if (Exists())
            {
                if (HasContent())
                {
                    RemoveContent();
                }
            }
            else
            {
                CreateFolder();
            }
            var serviceAccountSecId = GetNtAccount();
            var adminsSecId = GetAdminAccounts();
            SetPermissions(serviceAccountSecId, adminsSecId);
            DeployFiles();
        }

        internal bool Exists()
        {
            logger.LogInformation($"Check to see if {fullDestinationDirectory} exists");
            return Directory.Exists(fullDestinationDirectory);
        }

        internal bool HasContent()
        {
            logger.LogInformation($"Folder {fullDestinationDirectory} does exists");
            logger.LogInformation("Are files present?");
            return Directory.EnumerateFiles(fullDestinationDirectory).ToList().Count > 0;
        }

        internal void RemoveContent()
        {
            logger.LogInformation($"Deleting contents of {fullDestinationDirectory}");
            var di = new DirectoryInfo(fullDestinationDirectory);
            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                dir.Delete(true);
            }
            logger.LogInformation($"Contents of {fullDestinationDirectory} removed");
        }

        internal void DeployFiles()
        {
            logger.LogInformation($"Get files from {folderOptions.SourceDirectory}");
            var dir = new DirectoryInfo(folderOptions.SourceDirectory);

            logger.LogInformation($"Deploy files to {fullDestinationDirectory}");
            // Get the files in the directory and copy them to the new location.
            foreach (var file in dir.GetFiles())
            {
                string temppath = Path.Combine(fullDestinationDirectory, file.Name);
                file.CopyTo(temppath, false);
            }
        }

        internal void CreateFolder()
        {
            Directory.CreateDirectory(fullDestinationDirectory);
        }

        #region FolderPermissions
        internal void SetPermissions(SecurityIdentifier serviceAccountSecId, SecurityIdentifier adminsSecId)
        {
            string rootProgramFolder = Path.GetFullPath(Path.Combine(fullDestinationDirectory, @"..\..\"));
            var dInfo = new DirectoryInfo(rootProgramFolder);
            var dSecurity = dInfo.GetAccessControl();

            dSecurity.AddAccessRule(new FileSystemAccessRule(serviceAccountSecId, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
            dSecurity.AddAccessRule(new FileSystemAccessRule(adminsSecId, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));

            dInfo.SetAccessControl(dSecurity);
        }
        public SecurityIdentifier GetNtAccount()
        {
            NTAccount account = new NTAccount(accountOptions.ServiceAccountName);
            return (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
        }

        public SecurityIdentifier GetAdminAccounts()
        {
            //https://support.microsoft.com/en-us/help/243330/well-known-security-identifiers-in-windows-operating-systems
            //SID: S-1-5-32-544
            //Name: Administrators
            //Description: A built-in group.After the initial installation of the operating system, the only member of the group is the Administrator account.
            //   When a computer joins a domain, the Domain Admins group is added to the Administrators group.
            //   When a server becomes a domain controller, the Enterprise Admins group also is added to the Administrators group.
            var sIDWellKnown = new SecurityIdentifier("S-1-5-32-544");
            return new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, sIDWellKnown);
        }
        #endregion
    }
}
