using System;

namespace ComplianceSheriff.Logging
{
    public class APILoggerRequest
    {
        public string UserName { get; set; }
        public string APIEndpoint { get; set; }
        public string Method { get; set; }
        public string Organization { get; set; }
        public string Parameters { get; set; }
        public string RequestTime { get; set; }
        public string JsonWebToken { get; set; }
        public string UserAgent { get; set; }

        public override string ToString()
        {
            return string.Join(",",
           this.RequestTime,
           String.Format("\"{0}\"", this.Organization),
           String.Format("\"{0}\"", this.APIEndpoint),
           this.Method,
           String.Format("\"{0}\"", this.UserName),
           this.Parameters,
           this.UserAgent,
           String.Format("\"{0}\"", this.JsonWebToken));
        }
    }
}
