using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.RestApi.WebResponse
{
    public class ApiResponse
    {
        [JsonIgnore]
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        [JsonIgnore]
        public string StackTrace { get; set; }

        public ApiResponse(int statusCode = 0, string message = null)
        {
            StatusCode = statusCode;
            ErrorCode = !String.IsNullOrEmpty(message) ? message : GetDefaultMessageForStatusCode(statusCode);
        }

        private string GetDefaultMessageForStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case 200:
                    return String.Empty;
                case 400:
                    return "api:unknown:badRequest";
                case 401:
                    return "api:unknown:unauthorizedRequest";
                case 403:
                    return "api:unknown:forbidden";                
                case 404:
                    return "api:unknown:notFound";
                case 500:
                    return "api:unknown:serverError";
                case 502:
                    return "api:unknown:badGateway";
                case 503:
                    return "api:unknown:serviceUnavailable";
                case 504:
                    return "api:unknown:gatewayTimeout";
                default:
                    return String.Empty;
            }
        }
    }
}
