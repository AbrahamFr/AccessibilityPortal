using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ComplianceSheriff.RestApi.WebResponse
{
    public class ApiBadRequestResponse : ApiResponse
    {
        public List<string> Errors { get; }

        public ApiBadRequestResponse(ModelStateDictionary modelState)
            : base(400)
        {
            if (modelState.IsValid)
            {
                throw new ArgumentException("ModelState must be invalid", nameof(modelState));
            } else
            {
                if (modelState.ErrorCount > 0)
                {
                    if (Errors == null)
                    {
                        Errors = new List<string>();
                    }

                    foreach(ModelStateEntry entry in modelState.Values)
                    {
                        if (entry.Errors.Count > 0)
                        {
                            foreach (ModelError modelError in entry.Errors)
                            {
                                if (!String.IsNullOrWhiteSpace(modelError.ErrorMessage))
                                {
                                    Errors.Add(modelError.ErrorMessage);
                                } else
                                {
                                    if (modelError.Exception != null)
                                    {
                                        Errors.Add(modelError.Exception.Message);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
