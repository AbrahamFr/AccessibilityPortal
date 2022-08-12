using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Requests
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [RegularExpression("(?=.*[a-z])(?=.*[A-Z])+(?=.*[!@#$%^&*_-])+.{8,}", ErrorMessage = "{0}IsInvalid")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string TemporaryPassword { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string VerificationToken { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string OrganizationId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string AuthenticationType { get; set; }

    }
}
