using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class ImpactRange : IValidatableObject
    {

        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        public string MinImpact { get; set; }

        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        public string MaxImpact { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!String.IsNullOrWhiteSpace(MinImpact))
            {
                bool result = Int32.TryParse(MinImpact, out int number);

                if (!result || !Enumerable.Range(0, 101).Contains(number))
                {
                    yield return new ValidationResult("MinImpactRangeBetween0And100");
                }
            }

            if (!String.IsNullOrWhiteSpace(MaxImpact))
            {
                bool result = Int32.TryParse(MaxImpact, out int number);

                if (!result || !Enumerable.Range(0, 101).Contains(number))
                {
                    yield return new ValidationResult("MaxImpactRangeBetween0And100");
                }
            }

            if (!String.IsNullOrWhiteSpace(MinImpact) && !String.IsNullOrWhiteSpace(MaxImpact))
            {
                if (Convert.ToInt32(MaxImpact) <= Convert.ToInt32(MinImpact))
                {
                    yield return new ValidationResult("MinImpactMustBeLessThanMaxImpact");
                }                
            }
        }
    }
}
