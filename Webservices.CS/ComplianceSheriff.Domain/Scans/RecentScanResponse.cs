using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Text;

namespace ComplianceSheriff.Scans
{
    public class RecentScanResponse
    {
        public List<RecentScan> RecentScanList { get; set; }
        public int TotalRecords { get; set; }

        public RecentScanResponse()
        {
            this.RecentScanList = new List<RecentScan>();
        }
    }
}
