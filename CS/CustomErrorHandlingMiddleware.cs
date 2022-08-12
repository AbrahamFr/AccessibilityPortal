using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ComplianceSheriff.RestApi.WebResponse;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ComplianceSheriff.CS
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CustomErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next.Invoke(httpContext);
            }
            catch(Exception)
            {
                httpContext.Response.StatusCode = 500;
            }

            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.ContentType = "application/json";

                var response = new ApiResponse(httpContext.Response.StatusCode);

                var json = JsonConvert.SerializeObject(response);

                await httpContext.Response.WriteAsync(json);
            }

        }

        //SOURCE: https://stackoverflow.com/questions/38630076/asp-net-core-web-api-exception-handling
        //private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        //{
        //    var code = HttpStatusCode.InternalServerError; // 500 if unexpected

        //    if (exception is MyNotFoundException) code = HttpStatusCode.NotFound;
        //    else if (exception is MyUnauthorizedException) code = HttpStatusCode.Unauthorized;
        //    else if (exception is MyException) code = HttpStatusCode.BadRequest;

        //    var result = JsonConvert.SerializeObject(new { error = exception.Message });
        //    context.Response.ContentType = "application/json";
        //    context.Response.StatusCode = (int)code;
        //    return context.Response.WriteAsync(result);
        //}
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CustomErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomErrorHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomErrorHandlingMiddleware>();
        }
    }
}
