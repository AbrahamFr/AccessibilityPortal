using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Requests
{
    public class UpdatePasswordRequest
    {
        [Required(ErrorMessage="{0}IsRequired")]
        [RegularExpression("(?=.*[a-z])(?=.*[A-Z])+(?=.*[!@#$%^&*_-])+.{8,}", ErrorMessage = "{0}IsInvalid")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [RegularExpression("(?=.*[a-z])(?=.*[A-Z])+(?=.*[!@#$%^&*_-])+.{8,}", ErrorMessage = "{0}IsInvalid")]
        public  string NewPassword { get; set; }
    }
}
