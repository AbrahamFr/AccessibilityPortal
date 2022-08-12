using ServiceInstaller.Abstractions;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.ServiceProcess;

namespace ServiceInstaller
{
    [System.ComponentModel.Description("MSMQ: verify enabled and running")]
    public class MsmqManager : ISetupStep
    {
        private readonly ILogger<MsmqManager> logger;

        public MsmqManager(ILogger<MsmqManager> logger)
        {
            this.logger = logger;
        }
        public void Setup()
        {
            var services = ServiceController.GetServices().ToList();
            var msQue = services.Find(o => o.ServiceName == "MSMQ");
            if (msQue == null)
            {
                throw new InvalidOperationException("MSMQ needs to be enabled.");
            }
            if (msQue.Status != ServiceControllerStatus.Running)
            {
                throw new InvalidOperationException("MSMQ service needs to be started.");
            }
            logger.LogInformation("MSMQ is up and running.");
        }
    }
}
