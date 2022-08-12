using System;
using System.Text;
using System.Threading.Tasks;
using ComplianceSheriff.AdoNet;
using ComplianceSheriff.AdoNet.APILogger;
using ComplianceSheriff.APILogger;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.Configuration;
using ComplianceSheriff.ExceptionHandlers;
using ComplianceSheriff.FileSystem;
using ComplianceSheriff.JWTToken;
using ComplianceSheriff.Middleware;
using ComplianceSheriff.Scans;
using ComplianceSheriff.Urls;
using ComplianceSheriff.UsageAudit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CynthiaSays.CS
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile($"appsettings.local.json", optional: true)
                .AddJsonFile("authentication.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var keySecret = configuration["JwtSigningKey"];
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keySecret));

            services.AddControllers().AddNewtonsoftJson();

            services
                 .Configure<FormOptions>(x =>
                 {
                     x.MultipartBodyLengthLimit = int.MaxValue; //209715200;
                })
                 .Configure<ConfigurationOptions>(configuration)
                 .Configure<ConfigurationOptions>(configuration.GetSection("Shared"))
                 .AddComplianceSheriffDomainServices()
                 .AddTransient(_ => new JwtSignInHandler(symmetricKey))
                 .AddScoped<IAPILoggerMutator, APILoggerMutator>()
                 .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                 .AddScoped<IConnectionFactory, ConnectionFactory>()
                 .AddScoped<IUsageAuditService, UsageAuditService>()
                 .AddScoped<IScanService, ScanService>()
                 .AddScoped<IFileSystemService, FileSystemService>()
                 .AddScoped<IUrlServices, UrlServices>()
                 .AddScoped<ComplianceSheriff.Permission.IPermissionService, ComplianceSheriff.Permission.PermissionService>()
                 .AddScoped<ComplianceSheriff.Licensing.ILicensingService, ComplianceSheriff.Licensing.LicensingService>()
                 .AddComplianceSheriffRestApi(configuration.GetSection("ComplianceSheriff"))
                 .AddComplianceSheriffAdoNet(configuration.GetSection("ComplianceSheriff"))
                 .AddScanRuns(configuration)
                 .AddAuthorization(auth =>
                 {
                     var policy = new AuthorizationPolicyBuilder()
                         .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                         .RequireAuthenticatedUser().Build();
                     auth.AddPolicy("Bearer", policy);
                     auth.DefaultPolicy = policy;
                 })
                 .AddAuthentication(options =>
                 {
                    // This causes the default authentication scheme to be JWT.
                    // Without this, the Authorization header is not checked and
                    // you'll get no results. However, this also means that if
                    // you're already using cookies in your app, they won't be 
                    // checked by default.
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                 })
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters.ValidateLifetime = true;
                     options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                     options.TokenValidationParameters.IssuerSigningKey = symmetricKey;
                     options.TokenValidationParameters.ValidAudience = JwtSignInHandler.TokenAudience;
                     options.TokenValidationParameters.ValidIssuer = JwtSignInHandler.TokenIssuer;
                     options.TokenValidationParameters.LifetimeValidator = TokenLifetimeValidator.Validate;
                     options.Events = new JwtBearerEvents()
                     {
                         OnAuthenticationFailed = (context) =>
                         {
                             return Task.FromException(context.Exception);
                         }
                     };
                 });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            //Global Exception Handling
            //DO NOT Place any other use statements before this one
            app.UseExceptionHandler(
                builder =>
                {
                    builder.Run(
                            async context =>
                            {
                                string apiErrorMsg = String.Empty;
                                string logErrorMsg = String.Empty;

                                var ex = context.Features.Get<IExceptionHandlerFeature>();
                                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger<Startup>().LogError(ex.Error, ex.Error.Message);

                                var customErrorHandler = new CustomExceptionHandlerService();
                                var apiResponse = customErrorHandler.CustomApiExceptionHandler(ex);
                                context.Response.StatusCode = apiResponse.StatusCode;

                                var err = JsonConvert.SerializeObject(apiResponse);
                                context.Response.ContentType = "application/json";

                                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(err), 0, err.Length).ConfigureAwait(false);
                            });
                });

            app.Use(async (context, next) => {
                HttpRequestRewindExtensions.EnableBuffering(context.Request);
                await next();
            });

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseComplianceSheriffRestApi();

            app.UseStaticFiles();

            // Serve files from wwwroot with "History API" fallback (for SPA capabilities)
            app.UseRewriteUrl();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});
        }
    }
}
