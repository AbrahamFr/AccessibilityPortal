using ComplianceSheriff.Work;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ComplianceSheriff.Permission
{
    public interface IPermissionService
    {
        Task RecordUserPermission(Int32 userGroupId,
                                           string userPermissionType,
                                           string typeId,
                                           string actionType,
                                           IUnitOfWork unitOfWork);

        Task CreateUserGroupPermissionRecord(UserGroupPermission userGroupPermission, IUnitOfWork unitOfWork);

        Task DeleteUserGroupPermissionByUserGroupId(int userGroupId);

        UserGroupPermission BuildUserAuditPermission(Int32 userGroupId,
                                           string userPermissionType,
                                           string typeId,
                                           string actionType);
    }
}
