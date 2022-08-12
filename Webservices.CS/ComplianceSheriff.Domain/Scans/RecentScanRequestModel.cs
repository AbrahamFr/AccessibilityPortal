using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Scans
{
    public class RecentScanRequestModel
    {
        public string CurrentPage { get; set; }

        public string RecordsToReturn { get; set; }

        public int UserGroupId { get; set; }

        public string SortColumn { get; set; }

        public string SortDirection { get; set; }
    }
}
