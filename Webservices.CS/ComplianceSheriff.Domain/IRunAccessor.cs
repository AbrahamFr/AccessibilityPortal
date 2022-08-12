using ComplianceSheriff.Runs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff
{
    public interface IRunAccessor
    {
        Task<int> GetRunStatusByScanId(int scanId, CancellationToken cancellationToken);

        Task<Run> GetRunByRunId(int runId, CancellationToken cancellationToken);

        Task<int> GetLastCompletedRunIdForScanId(int scanId, CancellationToken cancellationToken);
    }
}
