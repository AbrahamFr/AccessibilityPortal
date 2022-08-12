using ComplianceSheriff.CheckpointGroups;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Scans
{
    public interface IScanAccessor
    {
        Task<List<Scan>> GetScansList(int userGroupId, CancellationToken cancellationToken);
        Task<Scan> GetScanById(int scanId, int userGroupId, CancellationToken cancellationToken);
        Task<List<Scan>> GetScanByName(string scanName, CancellationToken cancellationToken);
        Task<IEnumerable<int>> GetAllScanIdsByScanGroupId(int scanGroupId, CancellationToken cancellationToken);
        Task<int> GetResultViewIdByScanId(int scanId, CancellationToken cancellationToken);        
        string GetScanRecordForAudit(Scan scan);
        string GetScanChangesForAudit(Scan original, Scan updated, List<CheckpointGroupListItem> chkpointGroupList);
        Task<bool> CheckScanExistence(int scanId, CancellationToken cancellationToken);

        Task<RecentScanResponse> GetRecentScans(RecentScanRequestModel request, CancellationToken cancellationToken);

        Task<List<int>> GetScanIdList(int userGroupId, List<int> scanIds, CancellationToken cancellationToken);
    }
}
