using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class OccurrencePage
    {
        [IgnoreDataMember]
        public int RowNumber { get; set; }
        public string PageTitle { get; set; }
        public string PageUrl { get; set; }
        public int NoOfOccurrences { get; set; }
        public string CachedPageLink { get; set; }
        public List<OccurrencePageItem> Occurrences { get; set; }

        public OccurrencePage()
        {
            this.Occurrences = new List<OccurrencePageItem>();
        }
    }
}
