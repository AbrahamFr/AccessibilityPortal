using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.RestApi.Requests
{
    public class ScanByIdRequest
    {
        [Required]
        public int scanId { get; set; }

    }
}
