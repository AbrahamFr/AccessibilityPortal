using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Checkpoints
{
    public interface ICheckpointAccessor
    {        
        Task<IEnumerable<CheckpointFailure>> GetTop10CheckpointFailures(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true);

        Task<IEnumerable<CheckpointCheckpointGroupItem>> GetLicensedCheckpoints(string licensedModules, CancellationToken cancellationToken);
    }
}
