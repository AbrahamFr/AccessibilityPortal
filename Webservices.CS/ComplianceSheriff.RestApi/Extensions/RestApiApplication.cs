using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Builder
{
    public static class RestApiApplication
    {
        public static IApplicationBuilder UseComplianceSheriffRestApi(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
                endpoints.MapControllers());

            app.UseSwagger();
            app.UseSwaggerUI(c => 
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "Compliance Sheriff");
                c.DocumentTitle = "Title Documentation";
                c.DocExpansion(DocExpansion.None);
            });
            
            return app;
        }

    }
}
