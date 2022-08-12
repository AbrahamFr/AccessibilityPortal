using BasicEnvironment.Abstractions;
using System;
using System.Security.Principal;

namespace BasicEnvironment.Setup.SharedDirectory
{
    internal class NtAccounts
    {
        private readonly IClusterDataDirectoryOptions clusterOptions;
        private readonly Lazy<SecurityIdentifier> lazyClusterServiceAccount;

        public NtAccounts(IClusterDataDirectoryOptions clusterOptions)
        {
            this.clusterOptions = clusterOptions;
            this.lazyClusterServiceAccount = new Lazy<SecurityIdentifier>(() =>
            {
                NTAccount account = new NTAccount(clusterOptions.ServiceAccountName);
                return (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            });
        }

        public SecurityIdentifier ClusterServiceAccount => lazyClusterServiceAccount.Value;

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
    }
}