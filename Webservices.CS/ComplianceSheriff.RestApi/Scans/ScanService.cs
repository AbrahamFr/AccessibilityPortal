using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComplianceSheriff.Configuration;
using ComplianceSheriff.LogMessages;
using ComplianceSheriff.QueueServiceManager.Abstractions;
using ComplianceSheriff.ScanRuns;
using ComplianceSheriff.Work;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComplianceSheriff.Scans
{
    public class ScanService : IScanService
    {
        private readonly ILogger<ScanService> _logger;
        private readonly IScanMutator _scanMutator;
        private readonly IScanRunManager _scanRunManager;
        private readonly ILogMessagesMutator _logMessagesMutator;
        private readonly ConfigurationOptions _configurationOptions;


        public ScanService(ILogger<ScanService> logger,
                           IScanMutator scanMutator,
                           IScanAccessor scanAccessor,
                           IScanRunManager scanRunManager,
                           ILogMessagesMutator logMessagesMutator,
                           IOptions<ConfigurationOptions> configOptions)
        {
            _logger = logger;
            _scanMutator = scanMutator;
            _scanRunManager = scanRunManager;
            _logMessagesMutator = logMessagesMutator;
            _configurationOptions = configOptions.Value;
        }

        public async Task<int> RunScan(string orgId, 
                                  int scanId, 
                                  int? scanGroupId, 
                                  string username, 
                                  IUnitOfWorkFactory unitOfWorkFactory,                                   
                                  CancellationToken cancellationToken, 
                                  int maxSecondsToWait = 0, 
                                  int? scanGroupRunId = null, 
                                  bool isScheduledScan = false)
        {

            int newRunId = 0;

            try
            {
                //STEP 1: INSERT NEW RECORD INTO RUNS TABLE
                newRunId = await _scanMutator.AddScanRun(scanId, orgId, scanGroupId, scanGroupRunId, isScheduledScan);

                //STEP 2: Call QueueService API to Create a new Task
                var taskId = await _scanRunManager.CreateNewRunTask(orgId, scanId, newRunId);

                //STEP 3: Update RunsTable with new TaskId
                await _scanMutator.UpdateScanRun(newRunId, taskId, orgId);

                //STEP 4: SEND MESSAGE TO QUEUING SERVER
                _scanRunManager.SendTaskMessageToMSMQ(taskId);

                return newRunId;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.Message);

                //Log to LogMessages
                var logMessageItem = new LogMessagesItem
                {
                    Logger = String.Format("Run.{0}", newRunId),
                    Timestamp = DateTime.UtcNow,
                    Severity = 3,
                    Message = String.Format("ScanId: {0} returned the following error message: {1}", scanId, ex.Message)
                };

                _logMessagesMutator.AddLogMessagesRecord(logMessageItem);

                var abortReason = String.Format("Error Starting Scan: {0}", ex.Message);
                await _scanMutator.AbortScanRun(newRunId, (int)RunStatus.Aborted, abortReason);

                throw new HttpRequestException("unableToConnectToQueueService");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                //Log to LogMessages
                var logMessageItem = new LogMessagesItem
                {
                    Logger = String.Format("Run.{0}", newRunId),
                    Timestamp = DateTime.UtcNow,
                    Severity = 3,
                    Message = String.Format("ScanId: {0} returned the following error message: {1}", scanId, ex.Message)
                };

                _logMessagesMutator.AddLogMessagesRecord(logMessageItem);

                var abortReason = String.Format("Error Starting Scan: {0}", ex.Message);
                await _scanMutator.AbortScanRun(newRunId, (int)RunStatus.Aborted, abortReason);

                throw;
            }
        }
    }
}
