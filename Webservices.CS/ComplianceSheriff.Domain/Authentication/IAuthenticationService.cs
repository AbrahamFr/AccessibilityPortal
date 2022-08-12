using ComplianceSheriff.Enums;
using ComplianceSheriff.Users;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Authentication
{
    public interface IAuthenticationService
    {
        Task<User> AuthenticateOrgUser(string userName, string passWord, AuthenticationTypes authenticationType, CancellationToken cancellationToken);       

        public Task<AuthInfo> AuthenticateUser(string userName, string passWord, AuthenticationTypes authenticationType, CancellationToken cancellationToken);
    }
}
