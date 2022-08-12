using ComplianceSheriff.UserMapping;
using ComplianceSheriff.Users;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Authentication
{
    public interface IAuthenticator
    {
        Task<User> AuthenticateUser(string userName, string passWord, CancellationToken cancellationToken);       
    }
}
