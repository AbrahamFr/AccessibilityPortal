using ComplianceSheriff.Authentication;
using ComplianceSheriff.Factory;
using ComplianceSheriff.JWTToken;
using ComplianceSheriff.ModelState;
using ComplianceSheriff.QueueServiceManager;
using ComplianceSheriff.QueueServiceManager.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RestApiServices
    {
        public static IServiceCollection AddComplianceSheriffRestApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(option => option.EnableEndpointRouting = false)
                .AddApplicationPart(typeof(RestApiServices).Assembly);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "Compliance Sheriff", Version = "v1" });
                options.DescribeAllEnumsAsStrings();

                // Swagger 2.+ support 
                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"", 
                    In = "header",
                    Name = "Authorization",
                    Type = "apiKey"
                });

                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                });

                options.MapType<FileContentResult>(() => new Schema
                {
                    Type = "file",
                });

                options.OperationFilter<ComplianceSheriff.Filters.UploadOperationFilter>();
            });

            services.AddScoped<IModelStateService, ModelStateService>();
            services.AddScoped<IAuthenticationFactory, AuthenticationFactory>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IJsonWebTokenService, JsonWebTokenService>();
            services.AddScoped<IJWTTokenIdentityManagerNetCore, JWTTokentIdentityManagerNetCore>();

            return services;
        }

        public static IServiceCollection AddScanRuns(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IScanRunManager>(_ => new ScanRunManager(configuration["QueueServerBaseUrl"]));
            return services;
        }
    }
}
