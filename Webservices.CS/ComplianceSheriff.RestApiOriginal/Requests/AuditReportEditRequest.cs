using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Requests
{
    public class AuditReportEditRequest
    {
        public int AuditReportId { get; set; }
        public string ReportName { get; set; }
        public int AuditTypeId { get; set; }  
        public string ReportDescription { get; set; }  
    }
}
