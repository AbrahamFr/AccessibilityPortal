using CommandLine;
using ServiceInstaller.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceInstaller
{
    public class Options : IAllOptions
    {
        [Option('q', "quiet", Required = false, HelpText = "Run without prompts to the user.")]
        public bool Quiet { get; set; }

        [Option("locationPath", Required = false, Default = "", HelpText = "Path to a resource on the Controller/Host. Unix style path of form: path/to/resource.")]
        public string LocationPath { get; set; }

        [Option("locationZone", Required = false, Default = "local", HelpText = "Connect to server in local or via the Cloud.")]
        public string LocationZone { get; set; }

        [Option("clusterName", Required = false, Default = "ComplianceSheriff", HelpText = "Distinquish server farms or clusters.")]
        public string ClusterName { get; set; }

        [Option("serviceType", Required = true, HelpText = "Type of Windows Service to deploy.  \"Controller\" or \"Worker\"?")]
        public string ServiceType { get; set; }

        [Option("controllerMachineName", Required = true, HelpText = "Name of the server where the Shared Directory is located.")]
        public string ControllerMachineName { get; set; }

        [Option("destinationDirectory", Required = true, HelpText = "Destination of Application Files for the Service.")]
        public string DestinationDirectory { get; set; }

        [Option("sourceDirectory", Required = true, HelpText = "Source of Application Files for the Service.")]
        public string SourceDirectory { get; set; }

        [Option("serviceAccountName", Required = true, HelpText = "Name for the account that the Compliance Sheriff account will run under.")]
        public string ServiceAccountName { get; set; }
        [Option("serviceAccountPassword", Required = true, HelpText = "Password for the account that the Compliance Sheriff account will run under.")]
        public string ServiceAccountPassword { get; set; }
    }
}
