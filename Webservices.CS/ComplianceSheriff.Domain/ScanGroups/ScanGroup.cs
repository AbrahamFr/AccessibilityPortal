using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class ScanGroup
    {
        public int ScanGroupId { get; set; }
        public string DisplayName { get; set; }

        public DateTime? LastScanDate { get; set; }

        public StringCollection Scans = new StringCollection();
        public StringCollection Subgroups = new StringCollection();

    }
}
