using ComplianceSheriff.Work;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserInfos
{
    public interface IUserInfoMutator
    {
        Task AddUserInfo(UserInfo userInfo, CancellationToken cancellationToken);

        void UpdateUserInfo(UserInfo userInfo, IUnitOfWork unitOfWork, CancellationToken cancellationToken);
        Task RemoveUserInfo(string userInfoId, CancellationToken cancellationToken);
    }
}
