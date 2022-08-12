using ComplianceSheriff.Email;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Configuration
{
    public class ConfigurationOptions
    {
        public ConfigurationOptions()
        {

        }

        public  string SharedDir { get; set; }

        public string ClusterName { get; set; }

        public string SqlServer { get; set; }

        public string JWTExpirationInMinutes { get; set; }
        public string APILoggerConnection { get; set; }

        public string JwtSigningKey { get; set; }

        public string QueueServerBaseUrl { get; set; }

        public string AngularApplication { get; set; }

        public SmtpSettingsModel Smtp { get; set; }
    }
}
