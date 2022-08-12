using ComplianceSheriff.Enums;
using ComplianceSheriff.Factory;
using ComplianceSheriff.UserMapping;
using ComplianceSheriff.Users;
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

        public async Task<User> AuthenticateOrgUser(string userName, 
                                                 string passWord, 
                                                 AuthenticationTypes authenticationType,                                  
                                                 CancellationToken cancellationToken)
        {
            var authType = _authenticationFactory.Create(Enum.GetName(typeof(AuthenticationTypes), authenticationType));
            var user = await authType.AuthenticateUser(userName, passWord, cancellationToken);

            return user;
        }

        public async Task<AuthInfo> AuthenticateUser(string userName,
                                                 string passWord,
                                                 AuthenticationTypes authenticationType,
                                                 CancellationToken cancellationToken)
        {
            var authType = _authenticationFactory.Create(Enum.GetName(typeof(AuthenticationTypes), authenticationType));

            //Authenticate Org User
            var user = await authType.AuthenticateUser(userName, passWord, cancellationToken);

            //Is User Authenticated
            bool isAuthenticated = user != null;

            return new AuthInfo { IsAuthenticated = isAuthenticated, User = user };
        }
    }
}
