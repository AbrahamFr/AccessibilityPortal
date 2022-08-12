using ComplianceSheriff.Domain.Installers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Reflection;

namespace RestApiInstallerSetup
{
    public class Setup
    {        
        private readonly ILogger<Setup> logger;
        private readonly IServiceProvider services;
        private readonly IAllOptions serviceOptions;

        public Setup(IAllOptions serviceOptions, ILogger<Setup> logger, IServiceProvider services)
        {
            this.serviceOptions = serviceOptions;
            this.logger = logger;
            this.services = services;
        }

        public void SetupWebServices()
        {
            logger.LogDebug("Calling Setup methods");

            foreach (var setupStep in SetupSteps.All)
            {
                var comment = setupStep.GetCustomAttribute<DescriptionAttribute>()?.Description ?? setupStep.Name;
                logger.LogInformation("*****************************************************");
                logger.LogInformation($"Setup {comment} started...");
                (services.GetRequiredService(setupStep) as ISetupStep).Setup();
                logger.LogInformation($"Setup {comment} completed.");
            }

            // Install the Queue Service
            services.GetRequiredService<WindowsService>().InstallService();

            var queueServiceInstallCompleteMsg = String.Format("{0}{1}{2}{1}{0}", "***************************************", Environment.NewLine, "Queue Service Installation Successful !!!");
            logger.LogInformation(queueServiceInstallCompleteMsg);
        }
    }
}
