using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ComplianceSheriff.RestApi.Requests
{
    public class ScanByNameRequest
    {
        [Required]
        public string scanName { get; set; }
    }
}
