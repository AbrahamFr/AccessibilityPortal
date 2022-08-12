using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AuditReports
{
    public interface IAuditReportAccessor
    {
        Task<List<AuditReport>> GetAllAuditReports(int userGroupId, CancellationToken cancellationToken);

        Task<AuditReport> GetAuditReportById(int auditReportId, CancellationToken cancellationToken);

        Task<Int32> GetAuditReportId(string reportName, string reportLocation,CancellationToken cancellationToken);

        Task<AuditReport> GetAuditReportWithUserPermission(int userGroupId, int auditReportId, CancellationToken cancellationToken);

        Task<List<AuditReport>> GetAuditReportsByReportName(string reportName, CancellationToken cancellationToken);
    }
}
