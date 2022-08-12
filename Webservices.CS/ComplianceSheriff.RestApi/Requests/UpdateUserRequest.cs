using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ComplianceSheriff.Requests
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        public int? UserId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [RegularExpression("([a-zA-Z ]{1,})", ErrorMessage = "{0}MustBeAlphabeticOnly")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [RegularExpression("([a-zA-Z.' ]{1,})", ErrorMessage = "{0}MustBeAlphabeticOnly")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [EmailAddress(ErrorMessage = "{0}IsInvalid")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string TimeZone { get; set; }
    }
}
