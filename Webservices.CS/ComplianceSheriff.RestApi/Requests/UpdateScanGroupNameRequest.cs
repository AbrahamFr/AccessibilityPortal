using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Requests
{
    public class UpdateScanGroupNameRequest
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        public int? ScanGroupId { get; set; }
        [Required(ErrorMessage = "{0}IsRequired")]
        public string ScanGroupDisplayName { get; set; }
    }
}
