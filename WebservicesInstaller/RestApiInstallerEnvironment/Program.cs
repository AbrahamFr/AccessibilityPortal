using System;
using CommandLine;
using CommandLine.Text;
using ComplianceSheriff.Domain.Installers.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using NLog.Extensions.Logging;
using RestApiInstallerSetup;
using RestApiInstallerDomain;
using ApplicationPoolSecurity;
using SystemAccount;
using FileSecurity;
using FileSystem;
using ConfigurationFiles;
using WebServer;
using Formatters;

namespace RestApiInstallerEnvironment
{
    class Program
    {
        private static string appDirectory = String.Empty;

        static int Main(string[] args)
        {
            Console.Title = "Compliance Sheriff Web Services Installer";
            Console.WriteLine("Looking for .NET Framework...");

            bool hasProperVersion = VerifyDotNetFromRegistry();
            if (hasProperVersion)
            {
                Console.WriteLine("Compliance Sheriff Web Services Setup");
                var result = new Parser(with => with.IgnoreUnknownArguments = true)
                    .ParseArguments<Options>(args);
                appDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location
                        .Replace("RestApiInstallerEnvironment.exe", "")
                        .Replace("RestApiInstallerEnvironment.dll", "");
                Console.WriteLine($"Running in {appDirectory}");
                return result.MapResult(
                    parsedFunc: options =>
                    {
                        if (!AreRequiredArgumentsPresent(options))
                        {
                            Console.WriteLine("Add the values in and re-run the installer.");
                            return 1;
                        }
                        if (!AreArgumentsSanitized(options))
                        {
                            Console.WriteLine("Improper Arguments present, fix the error message and re-run the installer.");
                            return 1;
                        }
                        Console.WriteLine("Arguments have been read into Web Services Setup");
                        try
                        {
                            GetServiceProvider(options)
                                .GetRequiredService<Setup>()
                                    .SetupWebServices();
                            return 0;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message, ex, args);
                            return 1;
                        }
                        finally
                        {
                            NLog.LogManager.Shutdown();
                            if (!options.Quiet)
                            {
                                Console.WriteLine($"Check log files here: {appDirectory}logs");
                                Console.WriteLine("Press ANY key to exit the console.");
                                Console.ReadKey();
                            }
                        }
                    },
                    notParsedFunc: errors =>
                    {
                        ; Console.WriteLine(HelpText.AutoBuild(result).ToString());
                        Console.WriteLine("Press ANY key to exit the console.");
                        Console.ReadKey();
                        return 1;
                    });
            }
            else
            {
                Console.WriteLine("Please install the .NET Framework Version 4.7.2 or later");
                Console.ReadKey();
                return 1;
            }
        }


        private static IServiceProvider GetServiceProvider(IAllOptions serviceOptions)
        {
            var services = new ServiceCollection();

            services.AddLogging(configure => configure.AddConsole());
            services.AddLogging((builder) =>
                builder
                    .AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true })
                    .SetMinimumLevel(LogLevel.Trace)
            );

            var config = GetApplicationSettings();
            services.Configure<SystemConfiguration>(config);

            services.AddSingleton(serviceOptions);
            services.AddTransient<IClusterSettings, ClusterSettings>();
            services.AddTransient<IAccountOptions>(sp => sp.GetRequiredService<IAllOptions>());
            services.AddTransient<IFolderOptions>(sp => sp.GetRequiredService<IAllOptions>());
            services.AddTransient<IUrnOptions>(sp => sp.GetRequiredService<IAllOptions>());
            services.AddTransient<IApplicationPoolSecurityManager, ApplicationPoolSecurityManager>();
            services.AddTransient<IFileSecurityManager, FileSecurityManager>();
            services.AddTransient<IFileSystemManager, FileSystemManager>();
            services.AddTransient<ISystemAccountManager, SystemAccountManager>();
            services.AddTransient<IWebServerManager, WebServerManager>();
            services.AddTransient<IMessageFormatter, MessageFormatter>();
            services.AddTransient<DotNetFrameworkConfigEditor>();
            services.AddTransient<WebServiceConfigEditor>();
            services.AddTransient<QueueServiceConfigEditor>();
            services.AddTransient<JsonConfigEditor>();
            services.AddTransient<JwtSigningKeyConfigEditor>();
            services.AddTransient<WebServiceAppSettingsConfigEditor>();
            services.AddTransient(typeof(Setup));

            foreach (var setupStep in SetupSteps.All)
            {
                services.AddTransient(setupStep);
            }

            return services.BuildServiceProvider();
        }

        private static IConfigurationRoot GetApplicationSettings()
        {
            Console.WriteLine($@"{appDirectory}appsettings.json");
            return new ConfigurationBuilder()
                            .SetBasePath(appDirectory)
                            .AddJsonFile("appsettings.json")
                            .Build();
        }

        private static bool AreArgumentsSanitized(Options options)
        {
            const string errorPrefix = "Error: Option --";
            const string errorSuffix = " if provided cannot be white space string.";
            bool result = true;
            if (options.ClusterName.Trim().Equals(string.Empty))
            {
                Console.WriteLine($"{errorPrefix}clusterName{errorSuffix}");
                result = false;
            }
            if (options.SecretKey!=null && options.SecretKey.Trim().Equals(string.Empty))
            {
                Console.WriteLine($"{errorPrefix}clusterName{errorSuffix}");
                result = false;
            }
            return result;
        }

        private static bool AreRequiredArgumentsPresent(Options options)
        {
            const string errorPrefix = "Error: Option --";
            const string errorEmptyValue = " is required argument to run Webservices Installer and the value cannot be empty string.";
            bool result = true;
            if (string.IsNullOrWhiteSpace(options.TargetAppName))
            {
                Console.WriteLine($"{errorPrefix}targetAppName{errorEmptyValue}");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(options.ServiceAccountName))
            {
                Console.WriteLine($"{errorPrefix}serviceAccountName{errorEmptyValue}");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(options.ServiceAccountPassword))
            {
                Console.WriteLine($"{errorPrefix}serviceAccountPassword{errorEmptyValue}");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(options.SourceDirectory))
            {
                Console.WriteLine($"{errorPrefix}sourceDirectory{errorEmptyValue}");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(options.DestinationDirectory))
            {
                Console.WriteLine($"{errorPrefix}destinationDirectory{errorEmptyValue}");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(options.ControllerMachineName))
            {
                Console.WriteLine($"{errorPrefix}controllerMachineName{errorEmptyValue}");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(options.QueueServiceSourceDirectory))
            {
                Console.WriteLine($"{errorPrefix}queueServiceSourceDirectory{errorEmptyValue}");
                result = false;
            }
            return result;
        }

        #region Verify Dot NET Framework

        private static bool VerifyDotNetFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            string version = "";

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    version = CheckFor45PlusVersion((int)ndpKey.GetValue("Release"));
                    Console.WriteLine(".NET Framework Version: " + version);

                }
                else
                {
                    Console.WriteLine(".NET Framework Version 4.5 or later is not detected.");
                }
            }
            return version == "4.7.2 or later";
        }

        // Checking the version using >= will enable forward compatibility.
        private static string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 461808)
                return "4.7.2 or later";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }

        #endregion

    }
}
