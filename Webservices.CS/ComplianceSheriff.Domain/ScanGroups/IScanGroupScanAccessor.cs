using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ComplianceSheriff.Scans;

namespace ComplianceSheriff.ScanGroups
{
    public interface IScanGroupScanAccessor
    {
        Task<ScanGroupPerformanceMetrics> GetPagePerformanceMetrics(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true);

        Task<ScanGroupPerformanceMetrics> GetCheckPointPerformanceMetrics(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true);

        Task<IEnumerable<PageFailure>> GetTop10PageFailures(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true);

        Task<IEnumerable<ScanGroupHistory>> GetScanGroupHistory(int? scanGroupId, CancellationToken cancellationToken, bool scheduledScan = true);

        Task<IEnumerable<ScanPerformance>> GetScanPerformanceByScanGroup(int? scanGroupId, int? userGroupId, CancellationToken cancellationToken, bool scheduledScan = true);
    }
}
