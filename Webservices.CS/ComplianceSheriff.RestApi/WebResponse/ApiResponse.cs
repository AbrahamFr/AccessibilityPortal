using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
            return statusCode switch
            {
                200 => String.Empty,
                400 => "api:unknown:badRequest",
                401 => "api:unknown:unauthorizedRequest",
                403 => "api:unknown:forbidden",
                404 => "api:unknown:notFound",
                500 => "api:unknown:serverError",
                502 => "api:unknown:badGateway",
                503 => "api:unknown:serviceUnavailable",
                504 => "api:unknown:gatewayTimeout",
                _ => String.Empty,
            };
        }
    }
}
