using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.CheckpointGroups
{
    public interface ICheckpointGroupsAccessor
    {
        Task<CheckpointGroupings> GetCheckpointGroupings(CancellationToken cancellationToken);

        Task<List<CheckpointGroupListItem>> GetCheckpointGroupList(int userGroupId, string organizationId, CancellationToken cancellationToken);

        Task<List<CheckpointGroupListItem>> GetCheckpointGroupsBy(int userGroupId, string organizationId, int? scanId, int? scanGroupId, string checkpointGroupId, CancellationToken cancellationToken);
    }
}
