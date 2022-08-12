using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Requests
{
    public class CreateUserRequest : IValidatableObject
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        [RegularExpression("([a-zA-Z ]{1,})", ErrorMessage = "{0}MustBeAlphabeticOnly")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [RegularExpression("([a-zA-Z' ]{1,})", ErrorMessage = "{0}MustBeAlphabeticOnly")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [EmailAddress(ErrorMessage ="{0}IsInvalid")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "{0}MustBeAtLeast8Characters")]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[@$!% *#?&_])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9@$!% *#?&_])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[@$!% *#?&_])(?=.*?[^a-zA-Z0-9@$!% *#?&_])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[@$!% *#?&_])(?=.*?[^a-zA-Z0-9@$!% *#?&_])).{1,}$", ErrorMessage = "{0}MustMeetRequirements")]
        public string Password { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string OrganizationId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string UserGroupName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UserName == "\"\"")
            {
                yield return new ValidationResult("userNameIsRequired");
            }

            if (UserGroupName == "\"\"")
            {
                yield return new ValidationResult("userGroupNameIsRequired");
            }

            if (OrganizationId == "\"\"")
            {
                yield return new ValidationResult("organizationIdIsRequired");
            }           
        }
    }
}
