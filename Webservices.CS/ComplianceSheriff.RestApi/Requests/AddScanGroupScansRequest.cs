using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ComplianceSheriff.Requests
{
    public class AddScanGroupScansRequest
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        public int ScanGroupId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public List<int> ScanList { get; set; }
    }
}
