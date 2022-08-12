using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComplianceSheriff.ModelState
{
    public class ModelStateService : IModelStateService
    {
        public string InvalidResponseHandler(ModelStateDictionary modelState, string controllerName, string actionName)
        {
            var modelErrors = modelState.Values.SelectMany(e => e.Errors).ToList();
            var errorList = new List<string>();

            foreach (var error in modelErrors)
            {
                if (!String.IsNullOrWhiteSpace(error.ErrorMessage))
                {
                    errorList.Add(error.ErrorMessage);
                }

                if (error.Exception != null)
                {
                    if (error.Exception is Newtonsoft.Json.JsonReaderException)
                    {
                        errorList.Add("invalidJsonRequestBody");
                    }
                    else
                    {
                        errorList.Add(error.Exception.Message);
                    }
                }
            }

            var errorText = Char.ToLower(errorList.FirstOrDefault()[0]) + errorList.FirstOrDefault().Substring(1);
            controllerName = Char.ToLower(controllerName[0]) + controllerName.Substring(1);
            actionName = Char.ToLower(actionName[0]) + actionName.Substring(1);
            var responseCode = String.Format($"api:{controllerName}:{actionName}:invalidRequest:{errorText}");

            return responseCode;
        }
    }
}
