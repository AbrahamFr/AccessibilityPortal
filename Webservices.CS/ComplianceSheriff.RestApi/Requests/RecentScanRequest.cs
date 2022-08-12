using ComplianceSheriff.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ComplianceSheriff.Requests
{
    public class RecentScanRequest
    {
        [Required]
        public string CurrentPage { get; set; }

        [Required]
        public string RecordsToReturn { get; set; }

        [ValidValues("", "ScanId", "RunId", "ScanName", "Status", "Finished", "Started", "StartingUrl", "TotalPagesRan", "HealthScoreChangePercent", "CheckpointGroupDescription")]
        public string SortColumn { get; set; }

        [ValidValues("", "ASC", "DESC")]
        public string SortDirection { get; set; }

    }
}
