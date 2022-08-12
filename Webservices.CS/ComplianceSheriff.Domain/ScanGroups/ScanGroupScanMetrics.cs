using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class ScanGroupScanMetrics
    {
        public int? ScanGroupId { get; set; }

        public List<ScanMetrics> ScanMetrics { get; set; }
    }
}
