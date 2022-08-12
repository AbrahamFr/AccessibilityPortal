using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ComplianceSheriff.Requests
{
    public class NewScanGroupRequest : IValidatableObject
    {
        [Required(ErrorMessage ="{0}IsRequired")]
        public string ScanGroupName { get; set; }
        public string SetAsDefault { get; set; } = "false";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!String.IsNullOrWhiteSpace(SetAsDefault))
            {
                bool result = Boolean.TryParse(SetAsDefault, out bool boolValue);
                if (!result)
                {
                    yield return new ValidationResult("setAsDefaultMustBeBoolean");
                }
            }
        }
    }
}
