using ComplianceSheriff.UserAccounts;
using ComplianceSheriff.WebResponse;

namespace ComplianceSheriff.RestApi.WebResponse
{
    public interface IWebResponseService
    {
        public GenericApiResponse<string> CreateUserAccountManagerResponse(UserAccountManagerResponse userAccountManagerResponse,
                                                      string controllerName,
                                                      string actionName);

    }
}
