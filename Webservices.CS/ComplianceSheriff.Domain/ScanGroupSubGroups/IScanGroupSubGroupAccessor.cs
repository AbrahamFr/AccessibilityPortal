using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.ScanGroupSubGroups
{
    public interface IScanGroupSubGroupAccessor
    {
        Task<List<ScanGroupSubGroup>> GetScanGroupSubGroupsByScanGroupId(int scanGroupId, CancellationToken cancellationToken);
    }
}
