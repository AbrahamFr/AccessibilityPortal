using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace ComplianceSheriff.Filters
{
    public class UploadOperationFilter : IOperationFilter
    {

        //public void Apply(Swashbuckle.AspNetCore.Swagger.Operation operation, OperationFilterContext context)
        //{
        //    if (operation.OperationId.ToLower() == "audituploadstream")
        //    {
        //        operation.Parameters.Clear();

        //        operation.Parameters.Add(new NonBodyParameter
        //        {
        //            Name = "fileName",
        //            In = "query",
        //            Description = "Filename",
        //            Required = true,
        //            Type = "string",
        //        });

        //        operation.Parameters.Add(new NonBodyParameter
        //        {
        //            Name = "file",
        //            In = "formData",
        //            Description = "Upload File",
        //            Required = true,
        //            Type = "file"
        //        });

        //        operation.Parameters.Add(new BodyParameter
        //        {
        //            Name = "reportName",
        //            In = "formData",
        //            Description = "Report Name",
        //            Required = true
        //        });

        //        operation.Parameters.Add(new BodyParameter
        //        {
        //            Name = "reportDescription",
        //            In = "formData",
        //            Description = "Report Description",
        //            Required = false
        //        });

        //        operation.Parameters.Add(new BodyParameter
        //        {
        //            Name = "reportType",
        //            In = "formData",
        //            Description = "Report Type",
        //            Required = true
        //        });

        //        operation.Consumes.Add("multipart/form-data");
        //    }
        //}
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;

            if (descriptor != null && descriptor.ActionName.ToLower() == "audituploadstream")
            {
                operation.Parameters.Clear();

                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "fileName",
                    In = ParameterLocation.Query,
                    Description = "Name of file to be uploaded.",
                    Required = true,
                    Schema = new OpenApiSchema()
                    {
                        Type = "string"
                    }
                });

                var uploadFileMediaType = new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema()
                    {
                        Type = "object",
                        Properties =
                        {
                            ["fileToUpload"] = new OpenApiSchema()
                            {                            
                                Description = "Upload File",
                                Type = "string",
                                Format = "binary"                              
                            },
                            ["reportName"] = new OpenApiSchema()
                            {
                                Description = "Name of Report",
                                Type = "text",
                                Format = "string"
                            },
                            ["reportDescription"] = new OpenApiSchema()
                            {
                                Description = "Additional Report Description",
                                Type = "text",
                                Format = "string"
                            },
                            ["reportType"] = new OpenApiSchema()
                            {
                                Description = "Type of Report",
                                Type = "text",
                                Format = "integer"
                            }
                        },
                            Required = new HashSet<string>()
                        {
                            "file",
                            "reportName",
                            "reportType"
                        }
                    }
                };

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = uploadFileMediaType
                    }
                };
            }
        }
    }
}