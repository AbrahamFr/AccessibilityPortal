using ComplianceSheriff.Authentication;
using ComplianceSheriff.Email;
using ComplianceSheriff.Factory;
using ComplianceSheriff.JWTToken;
using ComplianceSheriff.ModelState;
using ComplianceSheriff.QueueServiceManager;
using ComplianceSheriff.QueueServiceManager.Abstractions;
using ComplianceSheriff.RestApi.WebResponse;
using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.TextFormatter;
using ComplianceSheriff.TimeZones;
using ComplianceSheriff.UserGroups;
using ComplianceSheriff.WebResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RestApiServices
    {
        public static IServiceCollection AddComplianceSheriffRestApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers().AddApplicationPart(typeof(RestApiServices).Assembly);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Compliance Sheriff", Version = "v1" });

                // Swagger 2.+ support 
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                options.MapType<FileContentResult>(() => new OpenApiSchema
                {
                    Type = "file",
                });


                options.OperationFilter<ComplianceSheriff.Filters.UploadOperationFilter>();
            });

            services.AddSwaggerGenNewtonsoftSupport();

            services.AddScoped<IWebResponseService, WebResponseService>();
            services.AddScoped<IModelStateService, ModelStateService>();
            services.AddScoped<IAuthenticationFactory, AuthenticationFactory>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IJsonWebTokenService, JsonWebTokenService>();
            services.AddScoped<ITimeZoneService, TimeZoneService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IScanGroupService, ScanGroupService>();
            services.AddScoped<IJWTTokenIdentityManagerNetCore, JWTTokentIdentityManagerNetCore>();
            services.AddScoped<IUserGroupsService, UserGroupsService>();
            services.AddScoped<ITextFormatterService, TextFormatterService>();

            return services;
        }

        public static IServiceCollection AddScanRuns(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IScanRunManager>(_ => new ScanRunManager(configuration["QueueServerBaseUrl"]));
            return services;
        }
    }
}
