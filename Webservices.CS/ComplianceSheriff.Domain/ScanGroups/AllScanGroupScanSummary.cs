using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class AllScanGroupScanSummary
    {
        public int? ScanGroupId { get; set; }

        public string DisplayName { get; set; }

        public int PageScans { get; set; }

        public int PageFailures { get; set; }

        public int CheckpointScans { get; set; }

        public int CheckPointFailures { get; set; }
    }
}
