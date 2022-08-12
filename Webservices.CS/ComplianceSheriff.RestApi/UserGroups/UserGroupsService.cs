using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserGroups
{
    public class UserGroupsService : IUserGroupsService
    {
        private readonly IUserGroupMutator _userGroupMutator;
        private readonly IUserGroupAccessor _userGroupAccessor;

        public UserGroupsService([FromServices] IUserGroupAccessor userGroupAccessor,
                                 [FromServices] IUserGroupMutator userGroupMutator)
        {
            this._userGroupAccessor = userGroupAccessor;
            this._userGroupMutator = userGroupMutator;
        }

        public async Task<UserGroup> GetUserGroupById(int userGroupId, CancellationToken cancellationToken)
        {
            var userGroup = await this._userGroupAccessor.GetUserGroupById(userGroupId, cancellationToken);
            return userGroup;
        }

        public async Task UpdateUserGroupName(int userGroupId, string userGroupName, CancellationToken cancellationToken)
        {
            await this._userGroupMutator.UpdateUserGroupName(userGroupId, userGroupName);
        }
    }
}
