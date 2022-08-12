using ComplianceSheriff.Domain.Installers.Abstractions;
using ComplianceSheriff.Domain.Installers.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace RestApiInstallerSetup
{
    public class WindowsService : ISetupStep
    {
        private readonly IUrnOptions urnOptions;
        private readonly IAllOptions options;
        private readonly IFolderOptions folderOptions;
        private readonly IAccountOptions accountOptions;
        private readonly SystemConfiguration configuration;
        private readonly ILogger<WindowsService> logger;
        private readonly string csServiceName;

        public WindowsService(IUrnOptions urnOptions,
                                       IAllOptions options,
                                       IFolderOptions folderOptions,
                                       IAccountOptions accountOptions,
                                       IOptions<SystemConfiguration> configuration,
                                       ILogger<WindowsService> logger)
        {
            this.urnOptions = urnOptions;
            this.folderOptions = folderOptions;
            this.accountOptions = accountOptions;
            this.configuration = configuration.Value;
            this.logger = logger;
            this.options = options;
            csServiceName = $"{this.configuration.QueueServiceName}";
        }

        public void Setup()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (EventLog.SourceExists(csServiceName))
                {
                    EventLog.WriteEntry(
                        csServiceName,
                        $"Fatal Exception : {Environment.NewLine} {e.ExceptionObject}",
                        EventLogEntryType.Error);
                }
            };
            logger.LogInformation($"Search for existing {csServiceName} service");

            string serviceName = csServiceName;
            bool defaultClusterName = urnOptions.ClusterName == "ComplianceSheriff";
            if (!defaultClusterName)
            {
                serviceName += $".{urnOptions.ClusterName}";
            }

            var services = ServiceController.GetServices().ToList();
            ServiceController csService = services.Find(o =>
                o.ServiceName == serviceName
                );
            if (csService != null)
            {
                logger.LogInformation($"{csServiceName} service found");

                logger.LogInformation($"{csServiceName} service status is {csService.Status}");

                if (csService.Status != ServiceControllerStatus.Stopped)
                {
                    logger.LogInformation($"Attemping to stop {csServiceName} service");
                    csService.Stop();

                    logger.LogInformation($"Waiting for {csServiceName} service to shut down");

                    csService.WaitForStatus(ServiceControllerStatus.Stopped);
                    System.Threading.Thread.Sleep(30000);
                }

                logger.LogInformation($"{csServiceName} service has stopped");
                UnInstallService(csService.ServiceName);
            }
        }

        private void UnInstallService(string serviceName)
        {
            logger.LogInformation($"Deleting {serviceName}");
            // Execute sc.exe command
            using (Process process = new Process())
            {
                process.StartInfo.FileName = @"sc.exe";
                process.StartInfo.ArgumentList.Add("delete");
                process.StartInfo.ArgumentList.Add(serviceName);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                try
                {
                    process.Start();
                    logger.LogInformation("Waiting for service to be uninstalled");
                    HandleExitCode(process, "Start of Delete service");
                    Thread.Sleep(5000);
                    logger.LogInformation($"Deleted {serviceName}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"During service delete - Exception Message: {ex.Message}");
                    throw ex;
                }
            }
        }

        internal int InstallService()
        {
            string fullDestinationDirectory = Path.Combine(folderOptions.GetFullDestinationDirectory(configuration), "queueService");
            string processName = $@"{fullDestinationDirectory}\{csServiceName}.exe";
            if (File.Exists(processName))
            {
                string urn = $"urn:{urnOptions.LocationZone}:{urnOptions.ClusterName}:{urnOptions.ControllerMachineName}";
                if (!String.IsNullOrWhiteSpace(urnOptions.LocationPath))
                {
                    urn = $"{urn}:{urnOptions.LocationPath}";
                }

                string serviceDisplayName = "Compliance Sheriff Queue Service";
                string serviceDescription = "The Compliance Sheriff Queue Service exposes an OWIN WebApi which provides access to MSMQ Services.";
                string serviceName = csServiceName;
                logger.LogInformation($"Installing {serviceName}");
                bool defaultClusterName = urnOptions.ClusterName == "ComplianceSheriff";
                if (!defaultClusterName)
                {
                    serviceName += $".{urnOptions.ClusterName}";
                    serviceDisplayName += $" ({urnOptions.ClusterName})";
                }

                return ExecuteServiceController(serviceName, $"\"{processName}\" -urn {urn}", serviceDisplayName, serviceDescription);
            }
            logger.LogInformation($"Service executable file not found at: {processName}");
            throw new Exception($"File for {csServiceName} not found");
        }

        /// <summary>
        /// https://support.microsoft.com/en-us/help/251192/how-to-create-a-windows-service-by-using-sc-exe
        /// PowerShell code
        ///     $binaryPathWithURN = $args[2];
        ///     $userName = $args[3];
        ///     $password = $args[4];
        ///     sc.exe create $serviceName binpath= $binaryPathWithURN DisplayName= $serviceDisplayName start= auto
        ///     sc.exe config $serviceName obj= $userName password= $password
        ///     sc.exe description $serviceName $serviceDescription
        ///     sc.exe failure $serviceName reset= 36000 actions= restart/15000////

        ///     sc.exe start $serviceName
        /// 	"Service Installed"
        /// </summary>
        /// <param name="servcieName"></param>
        /// <param name="action"></param>
        private int ExecuteServiceController(string serviceName, string binaryPathWithURN, string serviceDisplayName, string serviceDescription)
        {
            // Execute sc.exe command
            using (Process process = new Process())
            {
                process.StartInfo.FileName = @"sc.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                try
                {
                    string installStep = "create";
                    process.StartInfo.ArgumentList.Add(installStep);
                    process.StartInfo.ArgumentList.Add(serviceName);
                    process.StartInfo.ArgumentList.Add("binpath=");
                    process.StartInfo.ArgumentList.Add(binaryPathWithURN);
                    process.StartInfo.ArgumentList.Add("DisplayName=");
                    process.StartInfo.ArgumentList.Add(serviceDisplayName);
                    process.StartInfo.ArgumentList.Add("start=");
                    process.StartInfo.ArgumentList.Add("auto");
                    process.Start();
                    logger.LogInformation("Waiting for service to be created");
                    HandleExitCode(process, installStep);

                    installStep = "config";
                    process.StartInfo.ArgumentList.Add(installStep);
                    process.StartInfo.ArgumentList.Add(serviceName);
                    process.StartInfo.ArgumentList.Add("obj=");
                    process.StartInfo.ArgumentList.Add(accountOptions.ServiceAccountName);
                    process.StartInfo.ArgumentList.Add("password=");
                    process.StartInfo.ArgumentList.Add(accountOptions.ServiceAccountPassword);
                    process.Start();
                    logger.LogInformation("Waiting for service to be configured");
                    HandleExitCode(process, installStep);

                    installStep = "description";
                    process.StartInfo.ArgumentList.Add(installStep);
                    process.StartInfo.ArgumentList.Add(serviceName);
                    process.StartInfo.ArgumentList.Add(serviceDescription);
                    process.Start();
                    logger.LogInformation("Waiting for service to have description");
                    HandleExitCode(process, installStep);

                    installStep = "failure";
                    process.StartInfo.ArgumentList.Add(installStep);
                    process.StartInfo.ArgumentList.Add(serviceName);
                    process.StartInfo.ArgumentList.Add("reset=");
                    process.StartInfo.ArgumentList.Add("36000");
                    process.StartInfo.ArgumentList.Add("actions=");
                    process.StartInfo.ArgumentList.Add("restart/15000////");
                    process.Start();
                    logger.LogInformation("Waiting for service to be handle failure");
                    HandleExitCode(process, installStep);

                    installStep = "start";
                    process.StartInfo.ArgumentList.Add(installStep);
                    process.StartInfo.ArgumentList.Add(serviceName);
                    process.Start();
                    logger.LogInformation("Waiting for service to start");
                    HandleExitCode(process, installStep);

                    logger.LogInformation($"Installed {serviceName}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"During service installation - Exception Message: {ex.Message}");
                    throw ex;
                }

                logger.LogInformation($"{csServiceName} has been installed with Exit Code: {process.ExitCode}");
                return process.ExitCode;

            }
        }

        private void HandleExitCode(Process process, string installStep)
        {
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var errorMessage = $"Process failed at step {installStep} with the exitcode: {process.ExitCode}";
                logger.LogError(errorMessage);
                switch (process.ExitCode)
                {
                    case 1064:
                        logger.LogError("Error: An exception occurred in the service when handling the control request.");
                        break;
                    case 1069:
                        logger.LogError("Error: The service did not start due to a logon failure. Make Sure The Service Account Has Logon As Service Privilege.");
                        break;
                    case 1070:
                        logger.LogError("Error: After starting, the service hung in a start-pending state.");
                        break;
                    case 1072:
                        logger.LogError("Error: The specified service has been marked for deletion.  Make sure to close the Service Application");
                        break;
                    default:
                        logger.LogError("Error: unknown to Complinace Sheriff at this time.");
                        break;
                }
                throw new Exception(errorMessage);
            }
            process.StartInfo.ArgumentList.Clear();
        }
    }
}
