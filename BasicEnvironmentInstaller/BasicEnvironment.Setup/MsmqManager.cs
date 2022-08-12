using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Linq;
using System.ServiceProcess;
using BasicEnvironment.Abstractions;

namespace BasicEnvironment.Setup
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
            List<ServiceController> services = ServiceController.GetServices().ToList();
            ServiceController msQue = services.Find(o => o.ServiceName == "MSMQ");
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
