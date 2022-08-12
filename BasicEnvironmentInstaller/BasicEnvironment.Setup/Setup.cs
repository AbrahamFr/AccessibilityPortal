using BasicEnvironment.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Reflection;

namespace BasicEnvironment.Setup
{
    public class Setup
    {
        private readonly IClusterAllOptions clusterOptions;
        private ILogger<Setup> logger;
        private readonly IServiceProvider services;

        public Setup(IClusterAllOptions clusterOptions, ILogger<Setup> logger, IServiceProvider services)
        {
            this.clusterOptions = clusterOptions;
            this.logger = logger;
            this.services = services;
        }

        public void SetupBasicEnvironment()
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

            logger.LogInformation("*****************************************************");
            logger.LogInformation("Basic Environment Setup has completed!");
            logger.LogInformation("*****************************************************");
        } 
    }
}
