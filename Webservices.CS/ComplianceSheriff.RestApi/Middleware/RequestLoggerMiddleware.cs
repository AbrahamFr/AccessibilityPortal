using ComplianceSheriff.APILogger;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ComplianceSheriff.Middleware
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggerMiddleware> _logger;

        public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context, JwtSignInHandler jwtSignInHandler, IAPILoggerMutator mutator)
        {
            var request = context.Request;
            JwtPayload jwtPayload = null;
            JObject jObject = null;

            var authHeader = request.Headers["Authorization"].ToString();

            if (!String.IsNullOrWhiteSpace(authHeader))
            {
                jwtPayload = RestAPIUtils.GetJwtPayload(context, jwtSignInHandler);
            }
                
            var body = await ReadRequestBody(request);

            if (!String.IsNullOrEmpty(body))
            {
                try
                {
                    jObject = JObject.Parse(body);
                }
                catch(JsonReaderException ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
                           
            var requestObject = new APILoggerRequest
            {
                APIEndpoint = request.Path,
                UserName = jwtPayload != null ? jwtPayload["userName"].ToString() : "",
                Parameters = RemovePassword(body),
                Organization = jwtPayload != null ? jwtPayload["organizationId"].ToString() : 
                               jObject != null ? jObject.GetValue("organizationId") != null ? jObject.GetValue("organizationId", StringComparison.OrdinalIgnoreCase).ToString() : "" : "",
                Method = request.Method,
                JsonWebToken = authHeader,
                UserAgent = request.Headers["User-Agent"],
                RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            mutator.AddAPILogger(requestObject);

            await _next(context);
        }

        private string RemovePassword(string paramBody)
        {            
            var emptyPasswordRegEx = @"\""password\"":\""[a-zA-Z0-9!@#$%^&*_]?[""\\+]+(,)";
            var jsonPasswordRegEx = @"\""password\"":\""[a-zA-Z0-9!@#$%^&*-_]+("",)";

            var emptyPasswordMatch = Regex.Match(paramBody, emptyPasswordRegEx);
            var jsonPasswordMatch = Regex.Match(paramBody, jsonPasswordRegEx);

            if (!emptyPasswordMatch.Success && !jsonPasswordMatch.Success)
            {
                return paramBody;
            }

            if (emptyPasswordMatch.Success)
            {
                return Regex.Replace(paramBody, emptyPasswordRegEx, "");
            }
            
            if (jsonPasswordMatch.Success)
            {
                return Regex.Replace(paramBody, jsonPasswordRegEx, "");
            }

            return "";
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer).Replace("\n", "").Replace("\t", "");

            //DO NOT REMOVE - Rewinds the request to allow processing of request to continue
            request.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }
    }
}
