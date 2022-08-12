using ComplianceSheriff.Attributes;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class OccurrencesByPageRequest : OccurrencesRequestBase
    {
        [ValidValues("", "NoOfOccurrences", "PageTitle", "Url")]
        public override string SortColumn { get; set; }
    }
}
