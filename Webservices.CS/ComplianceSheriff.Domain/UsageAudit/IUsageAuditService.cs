using ComplianceSheriff.Work;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ComplianceSheriff.UsageAudit
{
    public interface IUsageAuditService
    {
        void RecordUserAction(string changeObject,
                                           string searchObject,
                                           string activityMessage,
                                           UserAuditType userAuditType,
                                           UserAuditActionType actionType,
                                           HttpContext context,
                                           IUnitOfWork unitOfWork);

        void CreateUserActionRecord(UserAudit userAudit, IUnitOfWork unitOfWork);

        UserAudit BuildUserAudit(string changeObject,
                                                  string searchObject,
                                                  string activity,
                                                  UserAuditType userAuditType,
                                                  UserAuditActionType actionType,
                                                  HttpContext context);
    }
}
