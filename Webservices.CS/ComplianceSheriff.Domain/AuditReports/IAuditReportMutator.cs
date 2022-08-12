using ComplianceSheriff.Work;
using System.Threading.Tasks;

namespace ComplianceSheriff.AuditReports
{
    public interface IAuditReportMutator
    {
        void AddAuditReport(AuditReport auditReport, IUnitOfWork unitOfWork);
        void UpdateAuditReport(AuditReport auditReport, IUnitOfWork unitOfWork);

        void DeleteAuditReport(int auditReportId, int userGroupId, IUnitOfWork unitOfWork);

        void UpdateAuditReportFileStatus(int auditReportId, int fileStatusId, IUnitOfWork unitOfWork);
    }
}
