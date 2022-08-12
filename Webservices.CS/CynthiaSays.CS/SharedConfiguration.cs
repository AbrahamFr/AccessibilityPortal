using ComplianceSheriff.AdoNet.Helpers;
using System;

namespace CynthiaSays.CS
{
    public class SharedConfiguration
    {
        public string SharedDir { get; set; }
        public string ClusterName { get; set; }
        public string CustomerName { get; set; }

        internal string GetSqlServerConnectionString()
        {
            if (String.IsNullOrWhiteSpace(this.SharedDir))
            {
                throw new ApplicationException("No Server Name supplied in application settings");
            }

            if (String.IsNullOrWhiteSpace(this.ClusterName))
            {
                this.ClusterName = "ComplianceSheriff";
            }

            return String.Format("Server={0};Database={1};Trusted_Connection=true;MultipleActiveResultSets=true",
                Helper.ReadXmlConfigForDBName(SharedDir, ClusterName), String.Format("{0}_{1}", this.ClusterName, this.CustomerName));
        }
    }
}
