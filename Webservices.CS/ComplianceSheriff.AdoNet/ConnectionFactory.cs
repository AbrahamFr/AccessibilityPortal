using System;
using ComplianceSheriff.AdoNet.Authentication;
using ComplianceSheriff.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ComplianceSheriff.Authentication;

namespace ComplianceSheriff.AdoNet
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly ConfigurationOptions _configOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ConnectionFactory> _logger;
        private JwtSignInHandler _jwtSignInHandler;

        public ConnectionFactory(IHttpContextAccessor httpContextAccessor, IOptions<ConfigurationOptions> options, ILogger<ConnectionFactory> logger, JwtSignInHandler jwtSignInHandler)
        {
            _logger = logger;
            _jwtSignInHandler = jwtSignInHandler;
            _configOptions = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }



        public SqlConnection GetContextDBConnection()
        {
            var userContext = new UserContext(_configOptions, _jwtSignInHandler);
            userContext.RetrieveUserContextData(_httpContextAccessor.HttpContext);
            return new SqlConnection(userContext.ConnectionString);
        }
    }
}
