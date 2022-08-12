using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserGroups
{
    public interface IUserGroupsService
    {
        Task<UserGroup> GetUserGroupById(int userGroupId, CancellationToken cancellationToken);

        Task UpdateUserGroupName(int userGroupId, string userGroupName, CancellationToken cancellationToken);
    }
}
