using System;
using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Reporting
{
    public class ScanGroupMetricsRequest
    {
        public int? ScanGroupID { get; set; }

        //public DateTimeOffset? StartDate { get; set; }

        //public DateTimeOffset? EndDate { get; set; }
    }
}
