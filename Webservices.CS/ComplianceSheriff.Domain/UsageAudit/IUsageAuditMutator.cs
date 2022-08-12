using ComplianceSheriff.Work;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.UsageAudit
{
    public interface IUsageAuditMutator
    {
        void AddUsageAuditRecord(UserAudit userAudit, IUnitOfWork unitOfWork);
    }
}
