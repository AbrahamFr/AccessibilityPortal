using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Runs
{
    public class Run
    {
        public int RunId { get; set; }
        public int ScanId { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public int Status { get; set; }
        public string TaskId { get; set; }
        public string AbortReason { get; set; }
        public int? ScanGroupId { get; set; }
        public int? Health { get; set; }
        public int? ScanGroupRunId { get; set; }
        public int? ScheduledScan { get; set; }
    }
}
