using ComplianceSheriff.Enums;
using ComplianceSheriff.RestApi.WebResponse;
using ComplianceSheriff.UserAccounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.WebResponse
{
    public class WebResponseService : IWebResponseService
    {
        public GenericApiResponse<string> CreateUserAccountManagerResponse(UserAccountManagerResponse userAccountManagerResponse,
                                                      string controllerName,
                                                      string actionName)
        {
            var errorCode = string.Empty;
            var statusCode = 0;

            switch (userAccountManagerResponse.Status)
            {
                case UserAccountCreateStatus.UserGroupExists:
                    errorCode = "userGroupAlreadyExists";
                    statusCode = 409;
                    break;

                case UserAccountCreateStatus.UserExists:
                    errorCode = "userAlreadyExists";
                    statusCode = 409;
                    break;
            }

            return new GenericApiResponse<string>
            {
                ErrorCode = string.Format("api:{0}:{1}:{2}", controllerName, actionName, errorCode),
                StatusCode = statusCode
            };
        }
    }
}
