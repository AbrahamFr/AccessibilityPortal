using ComplianceSheriff.Authentication;
using ComplianceSheriff.UserMapping;
using ComplianceSheriff.Users;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Authenticators
{
    public class FormsAuthentication : IAuthenticator
    {
        private readonly ILogger<FormsAuthentication> _logger;
        private readonly IAuthAccessor _authAccessor;

        public FormsAuthentication(ILogger<FormsAuthentication> logger,
                                   IAuthAccessor authAccessor)
        {
            _logger = logger;
            _authAccessor = authAccessor;
        }
        public async Task<User> AuthenticateUser(string userName, string passWord,                                                
                                                 CancellationToken cancellationToken)
        {
            var user = await _authAccessor.AuthenticateUser(userName, passWord, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning($"UserName {userName} does not exist or is not authenticated.");
            }

            return user;
        }
    }
}
