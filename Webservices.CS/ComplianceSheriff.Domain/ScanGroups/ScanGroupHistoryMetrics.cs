using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class ScanGroupHistoryMetrics
    {
        public DateTime FinishDate { get; set; }
        public int ScanRowIndex { get; set; }
        public PerformanceMetrics Metrics { get; set; }
    }
}
