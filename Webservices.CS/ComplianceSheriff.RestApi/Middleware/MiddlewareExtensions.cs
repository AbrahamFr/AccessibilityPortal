using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace ComplianceSheriff.Middleware
{ 
    public static class RewriteUrlMiddlewareExtensions
    {
        public static IApplicationBuilder UseRewriteUrl(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RewriteUrlMiddleware>();
        }
    }

    public static class RequestLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLoggerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggerMiddleware>();
        }
    }

    public static class RawRequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseRawRequestMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RawRequestMiddleware>();
        }
    }
}
