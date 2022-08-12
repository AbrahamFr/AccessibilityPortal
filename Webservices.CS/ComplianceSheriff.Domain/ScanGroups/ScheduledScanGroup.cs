using System;

namespace ComplianceSheriff.ScanGroups
{
    public class ScheduledScanGroup
    {
        public int ScanGroupId { get; set; }
        public string DisplayName { get; set; }
        public DateTime? LastScanDate { get; set; }
    }
}
