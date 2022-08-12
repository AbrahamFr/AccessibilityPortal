using ComplianceSheriff.Scans;
using System.Collections.Generic;

namespace ComplianceSheriff.ScanGroups
{
    public class SubGroupScansResponse
    {
        public List<ScanGroupListItem> SubGroups { get; set; }
        public List<ScanListItem> Scans { get; set; }

        public SubGroupScansResponse()
        {
            this.SubGroups = new List<ScanGroupListItem>();
            this.Scans = new List<ScanListItem>();
        }

    }
}
