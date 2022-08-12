using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class Metrics
    {
        public int ScanTotal { get; set; }
        public double SuccessPercent { get; set; }
        public int ScanFailure { get; set; }
        public double FailurePercent { get; set; }
    }
}
