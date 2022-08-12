using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserGroups
{
    public interface IUserGroupAccessor
    {
        Task<UserGroup> GetUserGroupByUserName(string userName, CancellationToken cancellationToken);

        Task<UserGroup> GetUserGroupByUserGroupName(string userGroupName, CancellationToken cancellationToken);

        Task<UserGroup> GetUserGroupById(int userGroupId, CancellationToken cancellationToken);
    }
}
