using ComplianceSheriff.Work;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace ComplianceSheriff.Scans
{
    public interface IScanMutator
    {
        Task<int> InsertScanAndDependencies(ScanRequest request, int userGroupId, string userPermissionType, string actionType);
        void AddScanCheckpointGroupIds(int scanId, List<string> checkPointGroupIds, IUnitOfWork unitOfWork);
        Task<bool> UpdateScanAndCheckpointGroups(int scanId, ScanUpdateRequest request);
        void UpdateScan(ScanUpdateRequest request,IUnitOfWork unitOfWork);
        Task<bool> DeleteScanAndDependencies(int scanId, int lastCompletedRunId);
        void DeleteScanCheckpointGroups(int scanId,IUnitOfWork unitOfWork);
        Task<int> AddScanRun(int scanId, string orgId, int? scanGroupId, int? scanGroupRunId, bool scheduledScan);        

        Task UpdateScanRun(int runId, string taskId, string orgId);

        Task AbortScanRun(int runId, int status, string abortReason);

        bool DeleteScanResultFiles(int runId, string organizationId, CancellationToken cancellationToken);
    }
}