using System.Runtime.Serialization;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class OccurrencePageItem
    {
        [IgnoreDataMember]
        public int RowNumber { get; set; }
        public int ScanId { get; set; }
        public string ScanDisplayName { get; set; }
        public string KeyAttribute { get; set; }

        [IgnoreDataMember]
        public string PageUrl { get; set; }
        public int? LineNumber { get; set; }
        public int? ColumnNumber { get; set; }
        public string Element { get; set; }
        public string CachedPageLink { get; set; }
        public string ContainerId { get; set; }
        public string State { get; set; }
        public int? ResultId { get; set; }
    }
}
