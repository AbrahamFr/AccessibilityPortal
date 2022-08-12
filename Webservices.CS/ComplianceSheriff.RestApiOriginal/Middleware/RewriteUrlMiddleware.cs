using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Concurrent;

namespace ComplianceSheriff.Middleware
{
    public class RewriteUrlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConcurrentDictionary<string, string> htmlCache = new ConcurrentDictionary<string, string>();

        public RewriteUrlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var pathBase = context.Request.PathBase;

            using (StreamReader reader = new StreamReader($"{Environment.CurrentDirectory}\\wwwroot\\index.html"))
            {
                var content = await reader.ReadToEndAsync();

                //string newHtml = content.Replace("<base href=\"/\">", $"<base href='{pathBase}/'>");
                await context.Response.WriteAsync(content);
            }
        }
    }
}
