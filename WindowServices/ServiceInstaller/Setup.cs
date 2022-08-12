using System;
using System.ComponentModel;
using System.Reflection;
using ServiceInstaller.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ServiceInstaller
{
    internal class Setup
    {
        private IAllOptions serviceOptions;
        private readonly ILogger<Setup> logger;
        private readonly IServiceProvider services;

        public Setup(IAllOptions serviceOptions, ILogger<Setup> logger, IServiceProvider services)
        {
            this.serviceOptions = serviceOptions;
            this.logger = logger;
            this.services = services;
        }

        internal void SetupService()
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

            // Install the Controller Service
            services.GetRequiredService<WindowsService>().InstallService();

            logger.LogInformation("*****************************************************");
            logger.LogInformation($"The {serviceOptions.ServiceType} Services Setup has completed!");
            logger.LogInformation("*****************************************************");
        }
    }

}
