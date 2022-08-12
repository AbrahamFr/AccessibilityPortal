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
}
