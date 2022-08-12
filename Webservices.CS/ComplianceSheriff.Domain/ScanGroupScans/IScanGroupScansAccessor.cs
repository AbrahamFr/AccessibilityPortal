using ComplianceSheriff.ScanGroups;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.ScanGroupScans
{
    public interface IScanGroupScansAccessor
    {
        Task<ScanGroupScan> GetScanGroupByScanGroupIdAndScanId(int scanGroupId, int scanId, CancellationToken cancellationToken);
    }
}
