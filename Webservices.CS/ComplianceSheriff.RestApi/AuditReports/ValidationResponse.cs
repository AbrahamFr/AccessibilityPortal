using System;
using System.Collections.Generic;
using System.Text;
using ComplianceSheriff.RestApi.WebResponse;

namespace ComplianceSheriff.AuditReports
{
    public class ValidationResponse
    {
        public ApiResponse ApiResponse { get; set; }
        public string LogErrorMessage { get; set; }
        public int MyProperty { get; set; }
        public bool IsValid { get; set; }
        public string Reason { get; set; }

        public ValidationResponse()
        {
            this.ApiResponse = new ApiResponse();
            IsValid = true;
        }
    }
}
