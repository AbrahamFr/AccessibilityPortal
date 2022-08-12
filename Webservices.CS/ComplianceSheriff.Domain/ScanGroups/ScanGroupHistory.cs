using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class ScanGroupHistory
    {
        public int ScanGroupRunId { get; set; }
        public int ScanGroupId { get; set; }
        public DateTime RunDate { get; set; }
        public int TotalPages { get; set; }
        public int PassedPages { get; set; }
        public int FailedPages { get; set; }
        public decimal PassedPagePercent { get; set; }

        public decimal FailedPagePercent { get; set; }
        public int TotalCheckpoints { get; set; }
        public int PassedCheckpoints { get; set; }

        public int FailedCheckpoints { get; set; }
        public decimal PassedCheckpointPercent { get; set; }
        public decimal FailedCheckpointPercent { get; set; }
    }
}
