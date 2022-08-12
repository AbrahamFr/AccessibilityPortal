using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ComplianceSheriff.Configuration;

namespace ComplianceSheriff.Middleware
{
    public class RewriteUrlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConfigurationOptions _configurationOptions;

        public RewriteUrlMiddleware(RequestDelegate next, IOptions<ConfigurationOptions> options)
        {
            _next = next;
            _configurationOptions = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (StreamReader reader = new StreamReader($"{Environment.CurrentDirectory}\\wwwroot\\{_configurationOptions.AngularApplication}\\index.html"))
            {
                var content = await reader.ReadToEndAsync();
                await context.Response.WriteAsync(content);
            }
        }
    }
}
