using ComplianceSheriff.CheckpointGroups;
using ComplianceSheriff.Checkpoints;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class IssueTrackerResponse
    {
        public List<IssueTrackerReportItem> IssueTrackerList { get; set; }
        public List<CheckpointListItem> CheckpointList { get; set; }
        public int TotalPagesScanned { get; set; }
        public int TotalOccurrences { get; set; }
        public int TotalPagesImpacted { get; set; }
        public int TotalIssuesFound { get; set; }
        public int TotalFilteredRecords { get; set; }
        public int TotalFailedIssues { get; set; }
        public int TotalHighSeverityIssues { get; set; }

        public IssueTrackerResponse()
        {
            this.IssueTrackerList = new List<IssueTrackerReportItem>();
            this.CheckpointList = new List<CheckpointListItem>();
        }
    }
}
