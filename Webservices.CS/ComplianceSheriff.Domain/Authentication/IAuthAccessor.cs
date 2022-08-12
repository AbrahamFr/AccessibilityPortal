using ComplianceSheriff.UserMapping;
using ComplianceSheriff.Users;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Authentication
{
    public interface IAuthAccessor
    {
        //Task<User> GetUserByUserName(string userName, CancellationToken cancellationToken);
        //public Task<UserProfile> GetUserProfileByUserName(string userName, CancellationToken cancellationToken);
        Task<User> AuthenticateUser(string userName, string passWord, CancellationToken cancellationToken);
        //Task<UserMapper> AuthenticateUserProfile(string userName, CancellationToken cancellationToken);
    }
}
