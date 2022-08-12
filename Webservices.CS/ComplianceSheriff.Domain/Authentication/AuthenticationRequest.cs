using ComplianceSheriff.Attributes;
using ComplianceSheriff.Enums;
using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Authentication
{
    public class AuthenticationRequest
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = true, ErrorMessage ="{0}IsRequired"), DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string OrganizationId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [ValidValues("FormsAuthentication")]
        public string AuthenticationType { get; set; }
    }
}
