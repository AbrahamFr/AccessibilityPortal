using System.Threading.Tasks;

namespace ComplianceSheriff.ScanGroupScans
{
    public interface IScanGroupScansMutator
    {
        Task AddScanGroupScan(int scanGroupId, int scanId);
    }
}
