using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class ScanGroupPerformanceMetrics
    {
        public int ScanGroupId { get; set; }

        public string  PerformanceType { get; set; }

        public PerformanceMetrics Metrics { get; set; }
    }
}
