using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.Work;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceSheriff.UsageAudit
{
    public class UsageAuditService : IUsageAuditService
    {
        private readonly JwtSignInHandler _jwtSignInHandler;
        private readonly IUsageAuditMutator _usageAuditMutator;

        public UsageAuditService([FromServices] JwtSignInHandler jwtSignInHandler, 
                                 [FromServices] IUsageAuditMutator usageAuditMutator)
        {
            _jwtSignInHandler = jwtSignInHandler;
            _usageAuditMutator = usageAuditMutator;
        }

        public void RecordUserAction(string changeObject, 
                                           string searchObject, 
                                           string activityMessage, 
                                           UserAuditType userAuditType, 
                                           UserAuditActionType actionType, 
                                           HttpContext context,
                                           IUnitOfWork unitOfWork)
        {
            var activity = CustomStringFormatter(activityMessage, changeObject, userAuditType.ToString());
            var userAudit = BuildUserAudit(changeObject, searchObject, activity, userAuditType, actionType, context);
            CreateUserActionRecord(userAudit, unitOfWork);
        }

        private string CustomStringFormatter(string formatString, params string[] args)
        {
            System.Text.RegularExpressions.Regex curlyBracketRegex = new System.Text.RegularExpressions.Regex("\\{(.+?)\\}");
            var numberOfArguments = curlyBracketRegex.Matches(formatString).Count;

            var missingArgumentCount = numberOfArguments - args.Length;
            if (missingArgumentCount <= 0) //more argument or just enough
                return string.Format(formatString, args);

            args = args.Concat(Enumerable.Range(0, missingArgumentCount).Select(_ => string.Empty)).ToArray();
            return string.Format(formatString, args);

        }

        public void CreateUserActionRecord(UserAudit userAudit, IUnitOfWork unitOfWork)
        {
            _usageAuditMutator.AddUsageAuditRecord(userAudit, unitOfWork);
            //await unitOfWork.CommitAsync(CancellationToken.None);
        }

        public UserAudit BuildUserAudit(string changeObject, 
                                          string searchObject, 
                                          string activity, 
                                          UserAuditType userAuditType, 
                                          UserAuditActionType actionType, 
                                          HttpContext context)
        {
            var userAudit = new UserAudit
            {
                UserName = RestAPIUtils.GetJwtPayload(context, _jwtSignInHandler)["userName"].ToString(),
                ActionTypeId = (int)actionType,
                UserAuditTypeId = (int)userAuditType,
                Activity = activity,
                Changes = changeObject,
                SearchObject = searchObject,
                RecordDate = DateTime.Now,
                IsXml = false,
                AdditionalInformation = string.Empty
            };

            return userAudit;
        }
    }
}
