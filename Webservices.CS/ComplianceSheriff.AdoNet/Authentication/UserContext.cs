using System;
using Microsoft.AspNetCore.Http;
using ComplianceSheriff.Configuration;
using ComplianceSheriff.Authentication;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ComplianceSheriff.AdoNet.Authentication
{
    public class UserContext
    {
        private readonly ConfigurationOptions _configOptions;
        private readonly JwtSignInHandler _jwtSignInHandler;

        public UserContext(ConfigurationOptions configOptions, JwtSignInHandler jwtSignInHandler)
        {
            _configOptions = configOptions;
            this._jwtSignInHandler = jwtSignInHandler;
        }

        public string UserName { get; set; }
        public string ClusterName { get; set; }
        public string OrganizationId { get; set; }

        public string ServerName { get; set; }
        public string ConnectionString { get; set; }

        public UserContext RetrieveUserContextData(HttpContext context)
        {
            var jwtPayloadHandler = new JwtPayloadHandler(_jwtSignInHandler);
            var jwtPayload = jwtPayloadHandler.GetJwtPayload(context);

            //Retrieve data from JWT Token or Context
            if (jwtPayload == null)
            {
                var authRequest = GetAuthRequest(context);
                this.OrganizationId = authRequest.OrganizationId;
                this.UserName = authRequest.UserName;
            } else
            {
                this.OrganizationId = jwtPayload["organizationId"].ToString();
                this.UserName = jwtPayload["userName"].ToString();
            }
 
            //Retrieve data from appsettings.json
            this.ClusterName = _configOptions.ClusterName;

            //Retrieve data from shared directory
            this.ServerName = Helpers.Helper.ReadXmlConfigForDBName(_configOptions.SharedDir, this.ClusterName);

            this.ConnectionString = BuildConnectionString();

            return this;
        }

        private AuthenticationRequest GetAuthRequest(HttpContext httpContext)
        {
            var rawRequestBody = httpContext.Items["rawRequestBody"].ToString();
            var authRequest = JsonConvert.DeserializeObject<AuthenticationRequest>(rawRequestBody);
            return authRequest;
        }

        private string BuildConnectionString()
        {
            return String.Format("Server={0};Database={1}_{2};Trusted_Connection=true;MultipleActiveResultSets=true", this.ServerName, this.ClusterName, this.OrganizationId);         
        }
    }

}
