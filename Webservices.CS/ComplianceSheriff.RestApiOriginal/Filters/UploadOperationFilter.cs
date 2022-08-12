using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComplianceSheriff.Filters
{
    public class UploadOperationFilter : IOperationFilter
    {

        public void Apply(Swashbuckle.AspNetCore.Swagger.Operation operation, OperationFilterContext context)
        {
            if (operation.OperationId.ToLower() == "audituploadstream")
            {
                operation.Parameters.Clear();

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "fileName",
                    In = "query",
                    Description = "Filename",
                    Required = true,
                    Type = "string",
                });

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "file",
                    In = "formData",
                    Description = "Upload File",
                    Required = true,
                    Type = "file"
                });

                operation.Parameters.Add(new BodyParameter
                {
                    Name = "reportName",
                    In = "formData",
                    Description = "Report Name",
                    Required = true
                });

                operation.Parameters.Add(new BodyParameter
                {
                    Name = "reportDescription",
                    In = "formData",
                    Description = "Report Description",
                    Required = false
                });

                operation.Parameters.Add(new BodyParameter
                {
                    Name = "reportType",
                    In = "formData",
                    Description = "Report Type",
                    Required = true
                });

                operation.Consumes.Add("multipart/form-data");
            }
        }
    }
}
