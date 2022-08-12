using System;
using System.IO;
using System.Management;
using System.Security.AccessControl;
using System.Security.Principal;
using BasicEnvironment.Abstractions;
using Microsoft.Extensions.Logging;

namespace BasicEnvironment.Setup.SharedDirectory
{
    [System.ComponentModel.Description("UNC Path Setup")]
    internal class UncPathSetup : ISetupStep
    {
        private readonly string shareableDir;
        private readonly IClusterDataDirectoryOptions clusterOptions;
        private readonly ILogger<UncPathSetup> logger;
        private readonly NtAccounts ntAccounts;
        private readonly SystemConfiguration configuration;

        public UncPathSetup(
            IClusterDataDirectoryOptions clusterOptions, 
            ILogger<UncPathSetup> logger, 
            NtAccounts ntAccounts, 
            SystemConfiguration configuration)
        {
            this.configuration = configuration;
            this.shareableDir = Path.GetDirectoryName(clusterOptions.GetSharedFullDataDirectory(this.configuration));
            this.clusterOptions = clusterOptions;
            this.logger = logger;
            this.ntAccounts = ntAccounts;
        }

        public void Setup()
        {
            bool alreadyShared = CheckIfFolderShared(shareableDir);

            if (!alreadyShared)
            {
                //https://www.codeproject.com/Articles/18624/How-to-Share-Windows-Folders-Using-C
                // Create a ManagementClass object
                var managementClass = new ManagementClass("Win32_Share");

                // Create ManagementBaseObjects for in parameters
                ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");

                // Set the input parameters
                inParams["Description"] = "Compliance Sheriff share";
                inParams["Name"] = configuration.CompanyDirectoryName;
                logger.LogInformation($"Folder to share: {shareableDir}");
                inParams["Path"] = shareableDir;
                inParams["Type"] = 0x0; // Disk Drive

                inParams["Access"] = CreateSecurityDescriptor(ntAccounts.ClusterServiceAccount, ntAccounts.GetAdminAccounts(), clusterOptions.ServiceAccountName);

                // Invoke the method on the ManagementClass object
                ManagementBaseObject outParams = managementClass.InvokeMethod("Create", inParams, null);
                if ((uint)(outParams.Properties["ReturnValue"].Value) != 0)
                {
                    throw new Exception("Unable to share directory.");
                }
                logger.LogInformation($"{shareableDir} is shared as {configuration.CompanyDirectoryName}");
            }
        }

        private bool CheckIfFolderShared(string shareableDir)
        {
            bool alreadyShared = false;
            var searcher = new ManagementObjectSearcher($"select * from win32_share where Name = '{configuration.CompanyDirectoryName}'");
            foreach (ManagementObject share in searcher.Get())
            {
                string type = share["Type"].ToString();
                if (type == "0") // 0 = DiskDrive 
                {
                    string shareableFolderUNC = $"\"{configuration.CompanyDirectoryName}\"";
                    bool networkPathInUse = share.Path.RelativePath.Contains(shareableFolderUNC);
                    if (networkPathInUse)
                    {
                        logger.LogInformation($"{shareableFolderUNC} is already shared");
                        // If Network path name is in use, mark as already shared
                        alreadyShared = true;
                        string diskPath = share["Path"].ToString(); //getting share path
                        if (diskPath.ToLower() != shareableDir.ToLower())
                        {
                            // If a different folder is being shared with the same name
                            throw new Exception($"The \"{share["Path"].ToString()}\" folder is already being shared with the {shareableFolderUNC} name; which is different than given path of \"{shareableDir}\"");
                        }
                    }
                }
            }
            return alreadyShared;
        }

        private ManagementObject CreateSecurityDescriptor(SecurityIdentifier serviceAccountSecId, SecurityIdentifier adminsSecId, string csServiceAccountName)
        {
            ManagementObject serviceAccountACE = CreateAccessControlObject(serviceAccountSecId, csServiceAccountName);
            var adminsACE = CreateAccessControlObject(adminsSecId, csServiceAccountName);

            // Create a security descriptor
            ManagementObject securityDescriptor = new ManagementClass(new ManagementPath("Win32_SecurityDescriptor"), null);
            securityDescriptor["ControlFlags"] = 4; //SE_DACL_PRESENT
            securityDescriptor["DACL"] = new object[] { serviceAccountACE, adminsACE };
            logger.LogInformation($"Security Descriptor created for {csServiceAccountName} and Admins");
            return securityDescriptor;
        }

        private ManagementObject CreateAccessControlObject(SecurityIdentifier SecId, string csServiceAccountName)
        {
            // Get Binary of Security Identifier
            byte[] utenteSIDArray = new byte[SecId.BinaryLength];
            SecId.GetBinaryForm(utenteSIDArray, 0);
            // Create Trustee
            ManagementObject trustee = new ManagementClass(new ManagementPath("Win32_Trustee"), null);
            trustee["Name"] = csServiceAccountName;
            trustee["SID"] = utenteSIDArray;
            // Create an Access Control Entry object
            ManagementObject accessControlEntry = new ManagementClass(new ManagementPath("Win32_Ace"), null);
            accessControlEntry["AccessMask"] = 2032127;//Full access
            accessControlEntry["AceFlags"] = AceFlags.ObjectInherit | AceFlags.ContainerInherit; //propagate the AccessMask to the subfolders
            accessControlEntry["AceType"] = AceType.AccessAllowed;
            accessControlEntry["Trustee"] = trustee;

            logger.LogInformation($"Access Control Entry created for {csServiceAccountName} and Admins");
            return accessControlEntry;
        }

    }
}
