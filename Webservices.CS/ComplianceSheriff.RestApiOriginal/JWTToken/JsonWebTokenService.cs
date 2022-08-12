using ComplianceSheriff.Authentication;
using ComplianceSheriff.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.JWTToken
{
    public class JsonWebTokenService : IJsonWebTokenService
    {
        private readonly ILogger<JsonWebTokenService> _logger;
        private readonly ConfigurationOptions _configOptions;
        private readonly IJWTTokenIdentityManagerNetCore _jWTTokenIdentityManagerNetCore;

        public JsonWebTokenService(ILogger<JsonWebTokenService> logger,
                                   IOptions<ConfigurationOptions> configOptions,
                                   IJWTTokenIdentityManagerNetCore jWTTokenIdentityManagerNetCore)
        {
            _logger = logger;
            _configOptions = configOptions.Value;
            _jWTTokenIdentityManagerNetCore = jWTTokenIdentityManagerNetCore;
        }

        public async Task<string> BuildJwtToken(IPrincipal user, string organizationId, CancellationToken cancellationToken)
        {            
            var keySecret = _configOptions.JwtSigningKey;
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keySecret));

            var jwtExpiration = !String.IsNullOrWhiteSpace(_configOptions.JWTExpirationInMinutes) ? Convert.ToInt32(_configOptions.JWTExpirationInMinutes) : 5.00;
            var expires = DateTimeOffset.UtcNow.AddMinutes(jwtExpiration);

            _logger.LogInformation(String.Format("JwtExpiration: {0} ", jwtExpiration));

            var clusterName = _configOptions.ClusterName;
            var orgVirtualDirectory = clusterName == "ComplianceSheriff" ? $"ComplianceSheriff_{organizationId}" : organizationId;

            await _jWTTokenIdentityManagerNetCore.Load(user, organizationId, orgVirtualDirectory, cancellationToken);

            var jwtTokenIdentityManager = _jWTTokenIdentityManagerNetCore;
            var jwtHandler = new JwtSignInHandler(symmetricKey);

            return jwtHandler.BuildJwt(expires, _jWTTokenIdentityManagerNetCore);
        }
    }
}
