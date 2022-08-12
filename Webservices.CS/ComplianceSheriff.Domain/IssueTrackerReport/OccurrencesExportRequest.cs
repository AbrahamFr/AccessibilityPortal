using ComplianceSheriff.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class OccurrencesExportRequest : OccurrencesRequestBase
    {
        [JsonIgnore]
        public override string CurrentPage { get; set; }

        [JsonIgnore]
        public override string RecordsToReturn { get; set; }

        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        new public string IssueId { get; set; }

        [ValidValues("", "ContainerId", "Element", "Issue", "KeyAttrName", "PageTitle", "ScanDisplayName", "ScanId", "Url")]
        public override string SortColumn { get; set; }

        new public string CheckpointId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [ValidValues("csv", "xlsx", "xml", "json")]
        public string ExportFormat { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [MaxLength(250, ErrorMessage = "{0}LengthTooLong")]
        public string FileName { get; set; }

    }
}
