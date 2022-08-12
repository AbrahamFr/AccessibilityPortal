using ComplianceSheriff.LogMessages;
using ComplianceSheriff.QueueServiceManager.Abstractions;
using ComplianceSheriff.Work;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Scans
{
    public interface IScanService
    {
        Task<int> RunScan(string orgId, int scanId, int? scanGroupId, string username, IUnitOfWorkFactory unitOfWorkFactory, CancellationToken cancellationToken, int maxSecondsToWait = 0, int? scanGroupRunId = null, bool isScheduledScan = false);
    }
}
