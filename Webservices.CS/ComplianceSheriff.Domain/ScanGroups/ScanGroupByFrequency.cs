using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class ScanGroupByFrequency
    {
        public string Frequency { get; set; }
        public List<ScanGroupByFrequencyMetrics> Metrics { get; set; }

        public ScanGroupByFrequency()
        {
            this.Metrics = new List<ScanGroupByFrequencyMetrics>();
        }
    }

    public class ScanGroupByFrequencyMetrics
    {
        public string Period { get; set; }
        public int Scans { get; set; }
        public int FailedPages { get; set; }
    }
}
