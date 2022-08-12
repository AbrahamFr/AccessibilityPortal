using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ComplianceSheriff.Attributes
{
    public class ValidValuesAttribute : ValidationAttribute
    {
        string[] _args;
        readonly string validValues;

        public ValidValuesAttribute(params string[] args)
        {
            _args = args;           
            validValues = string.Join(",", _args.Select(item => "\'" + item + "\'"));
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (((string)value) == null || _args.Contains((string)value, StringComparer.OrdinalIgnoreCase))
                return ValidationResult.Success;
            return new ValidationResult($"invalidValueFor{validationContext.MemberName}");
        }
    }
}
