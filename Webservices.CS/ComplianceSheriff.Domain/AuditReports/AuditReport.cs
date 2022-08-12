using ComplianceSheriff.Enums;
using System;


namespace ComplianceSheriff.AuditReports
{
    public class AuditReport
    {
        public int AuditReportId { get; set; }
        public string ReportName { get; set; }
        public int AuditTypeId { get; set; }
        public DateTime FileUploadDate { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string ReportDescription { get; set; }
        public string FileLocation { get; set; }
        public string AuditTypeName { get; set; }
        public int FileStatusId { get; set; }
        public string FileStatusName { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public Boolean CanEdit { get; set; }
    }
}
