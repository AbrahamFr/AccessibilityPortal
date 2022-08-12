using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.ScanGroups
{
    public interface IScanGroupAccessor
    {
        Task<IEnumerable<ScanGroupReport>> GetScanGroupReport(int scanGroupId, DateRange range, CancellationToken cancellationToken);
        Task<IEnumerable<ScheduledScanGroup>> GetScheduledScanGroups(int userGroupId, CancellationToken cancellationToken, bool scheduledScan = true);
        Task<IEnumerable<ScanGroupListItem>> GetAllScanGroupListByPermission(int userGroupId, CancellationToken cancellationToken);
        Task<SubGroupScansResponse> GetSubGroupsAndScansByScanGroupId(int userGroupId, int scanGroupId, CancellationToken cancellationToken);
        Task<IEnumerable<ScanGroup>> GetAllScanGroups(CancellationToken cancellationToken);
        Task<ScanGroup> GetScanGroupByDisplayName(string displayName, CancellationToken cancellationToken);

        Task<ScanGroup> GetScanGroupByScanGroupId(int scanGroupId, CancellationToken cancellationToken);
    }
}
