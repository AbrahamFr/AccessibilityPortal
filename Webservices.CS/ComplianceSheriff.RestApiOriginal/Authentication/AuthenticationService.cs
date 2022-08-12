using ComplianceSheriff.Enums;
using ComplianceSheriff.Factory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IAuthenticationFactory _authenticationFactory;

        public AuthenticationService(ILogger<AuthenticationService> logger,
                                     IAuthenticationFactory authenticationFactory)
        {
            _logger = logger;
            _authenticationFactory = authenticationFactory;
        }

        public async Task<User> AuthenticateUser(string userName, 
                                                 string passWord, 
                                                 AuthenticationTypes authenticationType,                                  
                                                 CancellationToken cancellationToken)
        {
            var authType = _authenticationFactory.Create(Enum.GetName(typeof(AuthenticationTypes), authenticationType));
            var user = await authType.AuthenticateUser(userName, passWord, cancellationToken);

            return user;
        }
    }
}
