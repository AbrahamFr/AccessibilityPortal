using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.ApiRoles
{
    public interface IApiRoleAccessor
    {
        Task<IEnumerable<ApiRole>> GetApiRolesByUserGroupName(string userGroupName, CancellationToken cancellationToken);
    }
}
