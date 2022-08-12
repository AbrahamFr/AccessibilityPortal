using ComplianceSheriff.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class IssueTrackerExportRequest : IssueTrackerRequestBase
    {
        [JsonIgnore]
        public override string CurrentPage { get; set; }

        [JsonIgnore]
        public override string RecordsToReturn { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [ValidValues("csv", "xlsx", "xml", "json")]
        public string ExportFormat { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [MaxLength(250, ErrorMessage = "{0}LengthTooLong")]
        public string FileName { get; set; }
    }
}
