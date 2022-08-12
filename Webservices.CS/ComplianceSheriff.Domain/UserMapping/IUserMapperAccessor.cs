using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserMapping
{
    public interface IUserMapperAccessor
    {
        Task<UserMapper> GetUserMapperByOrgUserId(int orgUserId, CancellationToken cancellationToken);
    }
}
