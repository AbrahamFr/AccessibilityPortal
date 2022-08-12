using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceInstaller.Abstractions;
using System;
using System.ComponentModel;

namespace ServiceInstaller
{
    [Description("Ensure Basic Environment is Setup")]
    public class EnsureBasicEnvironment : ISetupStep
    {
        private readonly IUrnOptions urnOptions;
        private readonly SystemConfiguration configuration;
        private readonly ILogger<EnsureBasicEnvironment> logger;

        public EnsureBasicEnvironment(
            IUrnOptions urnOptions,
            IOptions<SystemConfiguration> configuration, 
            ILogger<EnsureBasicEnvironment> logger)
        {
            this.urnOptions = urnOptions;
            this.configuration = configuration.Value;
            this.logger = logger;
        }
        public void Setup()
        {
            string sharedFullDataDirectory = $@"\\{urnOptions.GetSharedFullDataDirectory(configuration)}";
            logger.LogInformation($"Shared Data Directory: {sharedFullDataDirectory}");
            if (!System.IO.Directory.Exists(sharedFullDataDirectory))
            {
                throw new InvalidOperationException($"Basic Environment must first be setup on the Controller Server: {urnOptions.ControllerMachineName}.");
            }

        }
    }
}
