using ComplianceSheriff.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ComplianceSheriff.Factory
{
    public class AuthenticationFactory : IAuthenticationFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthenticationFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IAuthenticator Create(string authenticatorName)
        {
            Type type = Type.GetType("ComplianceSheriff.Authenticators." + authenticatorName);
            return (IAuthenticator)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }
    }
}
