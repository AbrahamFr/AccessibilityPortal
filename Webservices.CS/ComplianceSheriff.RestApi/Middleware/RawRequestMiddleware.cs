using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ComplianceSheriff.Middleware
{
    public class RawRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public RawRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer).Replace("\n", "").Replace("\t", "");
            context.Items["rawRequestBody"] = bodyAsText;

            //DO NOT REMOVE - Rewinds the request to allow processing of request to continue
            request.Body.Seek(0, SeekOrigin.Begin);
        
            await _next(context);
        }
    }
}
