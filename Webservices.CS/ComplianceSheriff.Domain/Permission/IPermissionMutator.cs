using ComplianceSheriff.Work;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ComplianceSheriff.Permission
{
    public interface IPermissionMutator
    {
        void AddUserGroupPermissionRecord(UserGroupPermission userPermission, IUnitOfWork unitOfWork);

        Task DeleteUserGroupPermissionsByUserGroupId(int userGroupId);
    }
}
