using ComplianceSheriff.Users;
using ComplianceSheriff.Work;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserGroups
{
    public interface IUserGroupMutator
    {
        Task<int> AddUserGroup(string userGroupName);

        Task DeleteUserGroupById(int userGroupId);

        Task UpdateScanGroupId(int userGroupId, int scanGroupId);

        Task UpdateUserGroupName(int userGroupId, string userGroupName);
    }
}
