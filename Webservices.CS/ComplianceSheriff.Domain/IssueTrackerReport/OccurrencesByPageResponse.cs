using System.Collections.Generic;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class OccurrencesByPageResponse
    {
        public List<OccurrencePage> OccurrencePages { get; set; }
        public int TotalPages { get; set; }
        public int TotalFilteredPages { get; set; }

        public OccurrencesByPageResponse()
        {
            this.OccurrencePages = new List<OccurrencePage>();
        }
    }
}
