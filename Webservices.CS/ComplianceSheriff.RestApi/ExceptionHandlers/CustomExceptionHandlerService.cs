using ComplianceSheriff.Exceptions;
using ComplianceSheriff.RestApi.WebResponse;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace ComplianceSheriff.ExceptionHandlers
{
    public class CustomExceptionHandlerService
    {
        public ApiResponse CustomApiExceptionHandler(IExceptionHandlerFeature exceptionHandlerFeature)
        {
            var exception = exceptionHandlerFeature.Error;
            int statusCode = 0;
            var errMessage = String.Empty;

            switch (exception)
            {
                case SecurityTokenException s:
                     statusCode = (int)HttpStatusCode.Unauthorized;
                     break;

                case InvalidTokenException ite:
                    statusCode = (int)StatusCodes.Status400BadRequest;
                    errMessage = BuildCustomApiErrorMessage(exceptionHandlerFeature, ite.Message);
                    break;

                case HttpRequestException hre:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    errMessage = BuildCustomApiErrorMessage(exceptionHandlerFeature, hre.Message);
                    break;

                case NoLicenseKeyException le:
                    statusCode = (int)StatusCodes.Status424FailedDependency;
                    errMessage = BuildCustomApiErrorMessage(exceptionHandlerFeature, le.Message);
                    break;

                case SqlConnectionException sqlex:
                    statusCode = (int)StatusCodes.Status400BadRequest;
                    errMessage = BuildCustomApiErrorMessage(exceptionHandlerFeature, sqlex.Message);
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            return new ApiResponse(statusCode, errMessage);
        }

        private string BuildCustomApiErrorMessage(IExceptionHandlerFeature ex, string customErrorMessage)
        {
            char[] charSeparators = new char[] { '/' };
            var pathArray = ((ExceptionHandlerFeature)ex).Path.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            var pathArrayToLower = pathArray.Select(s => Char.ToLowerInvariant(s.FirstOrDefault()) + s.Substring(1));
            var requestPath = string.Join(":", pathArrayToLower).Replace("rest", "api");
            var apiErrorMsg = String.Format("{0}:{1}", requestPath, customErrorMessage);

            return apiErrorMsg;
        }        
    }
}
