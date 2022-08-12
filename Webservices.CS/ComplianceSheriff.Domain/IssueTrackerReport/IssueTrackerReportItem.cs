using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class IssueTrackerReportItem
    {
        public string Issue { get; set; }
        public string Severity { get; set; }
        public double Impact { get; set; }
        public int PriorityLevel { get; set; }
        public int HighestPageLevel { get; set; }
        public int Occurrences { get; set; }
        public int Pages { get; set; }
        public int Scans { get; set; }
        public string Checkpoint { get; set; }
        public string State { get; set; }
        public string CheckpointUrl { get; set; }
        public string CheckpointId { get; set; }
        public int IssueId { get; set; }
    }
}
