using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Requests
{
    public class ScanRunRequest
    {
        [Required(ErrorMessage ="{0}IsRequired")]
        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]
        public string ScanId { get; set; }

    }
}
