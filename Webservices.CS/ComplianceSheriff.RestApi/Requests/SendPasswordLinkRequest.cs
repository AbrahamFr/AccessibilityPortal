using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Requests
{
    public class SendPasswordLinkRequest
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        [EmailAddress(ErrorMessage = "{0}IsInvalid")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string OrganizationId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string AuthenticationType { get; set; }
    }
}
