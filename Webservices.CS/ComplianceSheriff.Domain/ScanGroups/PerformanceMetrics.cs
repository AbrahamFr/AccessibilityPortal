using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class PerformanceMetrics
    {
        public int ScanTotal { get; set; }
        public int PassedTotal { get; set; }
        public double PassedPercent { get; set; }
        public int FailedTotal { get; set; }
        public double FailedPercent { get; set; }
    }
}
