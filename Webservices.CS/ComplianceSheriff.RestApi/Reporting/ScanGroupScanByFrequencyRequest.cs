using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Reporting
{
    public class ScanGroupScanByFrequencyRequest
    { 
        public int? ScanGroupID { get; set; }

        public string Frequency { get; set; }

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }
    }
}
