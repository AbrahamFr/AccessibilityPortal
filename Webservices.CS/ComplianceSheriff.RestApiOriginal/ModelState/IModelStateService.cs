using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ComplianceSheriff.ModelState
{
    public interface IModelStateService
    {
        string InvalidResponseHandler(ModelStateDictionary modelState, string controllerName, string actionName);
    }
}
