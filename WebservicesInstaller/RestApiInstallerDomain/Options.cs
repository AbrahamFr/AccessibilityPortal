using CommandLine;
using ComplianceSheriff.Domain.Installers.Abstractions;

namespace RestApiInstallerDomain
{
    public class Options : IAllOptions
    {
        [Option('q', "quiet", Required = false, HelpText = "Run without prompts to the user.")]
        public bool Quiet { get; set; }

        [Option("clusterName", Required = false, Default = "ComplianceSheriff", HelpText = "Cluster Name.")]
        public string ClusterName { get; set; }

        [Option("secretKey", Required = false, HelpText = "JSON Web Token Secret key to be passed in by the user.")]
        public string SecretKey { get; set; }

        [Option("useWindowsAuthentication", Required = false, HelpText = "Connect to SQL via Windows Authentication during installation.")]
        public bool UseWindowsAuthentication { get; set; }

        [Option("databaseAdminName", Required = false, HelpText = "Database Connection User Name.")]
        public string DataBaseAdminName { get; set; }

        [Option("databaseAdminPassword", Required = false, HelpText = "Database Connection Password.")]
        public string DataBaseAdminPassword { get; set; }

        [Option("targetAppName", Required = true, HelpText = "Target Application to be installed.")]
        public string TargetAppName { get; set; }

        [Option("serviceAccountName", Required = true, HelpText = "Application Pool Username.")]
        public string ServiceAccountName { get; set; }

        [Option("serviceAccountPassword", Required = true, HelpText = "Application Pool Password.")]
        public string ServiceAccountPassword { get; set; }

        [Option("sourceDirectory", Required = true, HelpText = "Location of Source Files.")]
        public string SourceDirectory { get; set; }

        [Option("destinationDirectory", Required = true, HelpText = "Location of Destination Files.")]
        public string DestinationDirectory { get; set; }

        [Option("controllerMachineName", Required = true, HelpText = "Name of Server with Shared Directory.")]
        public string ControllerMachineName { get; set; }

        [Option("queueServiceSourceDirectory", Required = true, HelpText = "Location of Source Files for Queue Windows Service")]
        public string QueueServiceSourceDirectory { get; set; }

        [Option("locationPath", Required = false, Default = "", HelpText = "Path to a resource on the Controller/Host. Unix style path of form: path/to/resource.")]
        public string LocationPath { get; set; }

        [Option("locationZone", Required = false, Default = "local", HelpText = "Connect to server in local or via the Cloud.")]
        public string LocationZone { get; set; }

    }
}
