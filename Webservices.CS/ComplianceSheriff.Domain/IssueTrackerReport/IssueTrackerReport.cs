using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class IssueTrackerReport
    {
        public string ScanId { get; set; }
        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? Finished { get; set; }
        public int? Status { get; set; }
        public string TaskId { get; set; }
        public string AbortReason { get; set; }
        public int RunId { get; set; }
        public string ScanGroupId { get; set; }
        public int? Health { get; set; }
        public int? ScanGroupRunId { get; set; }
        public bool? ScheduledScan { get; set; }
        public int PageId { get; set; }
        public string Url { get; set; }
        public Int16 IsPage { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }
        public string CheckId { get; set; }
        public int Result { get; set; }
        public int Priority { get; set; }
        public int? Count { get; set; }
        public int ResultId { get; set; }
        public int? ResultTextId { get; set; }
        public int ValueId { get; set; }
        public string Issue { get; set; }
        public int CheckPointId { get; set; }
        public string Number { get; set; }
        public string CheckpointShortDescription { get; set; }
        public string CheckpointLongDescription { get; set; }
        public string CheckpointUrl { get; set; }
        public int CheckPointPriority { get; set; }
        public int CheckPointGroupId { get; set; }
        public string CheckpointGroupShortDescription { get; set; }
        public string CheckpointGroupLongDescription { get; set; }
        public string State { get; set; }
    }
}
