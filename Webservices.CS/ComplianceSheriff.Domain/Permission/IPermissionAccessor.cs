using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Permission
{
    public interface IPermissionAccessor
    {
        Task<Int32> GetUserGroupIdForPermission(string userName, CancellationToken cancellationToken);

        Task<bool> CheckScanRunPermission(int scanId, int userGroupId, CancellationToken cancellationToken);

        Task<bool> CheckPermissionTypeAndActionForUserGroup(int objectId, int userGroupId, string permissionType, string permissionAction, CancellationToken cancellationToken);
    }
}
