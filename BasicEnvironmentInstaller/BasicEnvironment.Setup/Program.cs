using System;
using System.Configuration;
using System.Collections.Specialized;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;
using BasicEnvironment.Abstractions;

namespace BasicEnvironment.Setup
{
    public class Program
    {
        private static string appDirectory = String.Empty;
        static int Main(string[] args)
        {
            Console.Title = "Compliance Sheriff Basic Environment Setup";
            Console.WriteLine("Begin setup of Basic Environment");
            appDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location
                        .Replace("BasicEnvironment.Setup.exe", "")
                        .Replace("BasicEnvironment.Setup.dll", "");
            Console.WriteLine($"In current directory: {appDirectory}");

            var result = new Parser(with => with.IgnoreUnknownArguments = true)
                .ParseArguments<Options>(args);

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
                    Console.WriteLine("Arguments have been read into Basic Environment Setup");
                    try
                    {
                        GetServiceProvider(options)
                            .GetRequiredService<Setup>()
                                .SetupBasicEnvironment();
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        // Temporary Fix - We should be using Logger Here - Will wait for more feedback as it was in place earlier.
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message, ex, args);
                        //logger.LogError(ex.Message, ex, args);
                        Console.ForegroundColor = ConsoleColor.Gray;
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
                        Console.WriteLine(HelpText.AutoBuild(result).ToString());
                        return 1;
                    });
        }

        public static string GetNameOfShareableFolder()
        {
            string shareableDirectory = string.Empty;
            var connectionManagerDatabaseServers = ConfigurationManager.GetSection("ConnectionManagerShareableFolder") as NameValueCollection;
            if (connectionManagerDatabaseServers != null)
            {
                shareableDirectory = connectionManagerDatabaseServers["directory"].ToString();
            }
            // The Following cannot be changed as its needed to support legacy customers
            return shareableDirectory ?? "Cryptzone";
        }

        private static IServiceProvider GetServiceProvider(IClusterAllOptions clusterOptions)
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());

            services.AddLogging((builder) =>
                builder
                    .AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true })
                    .SetMinimumLevel(LogLevel.Trace)
            );

            var systemConfig = GetApplicationSettings();
            services.AddSingleton(systemConfig);

            services.AddSingleton(clusterOptions);
            services.AddTransient<IClusterDataOptions>(sp => sp.GetRequiredService<IClusterAllOptions>());
            services.AddTransient<IClusterDataDirectoryOptions>(sp => sp.GetRequiredService<IClusterAllOptions>());
            services.AddSingleton<SharedDirectory.NtAccounts>();

            services.AddTransient(typeof(Setup));
            foreach (var setupStep in SetupSteps.All)
            {
                services.AddTransient(setupStep);
            }

            return services.BuildServiceProvider();
        }
        private static SystemConfiguration GetApplicationSettings()
        {
            string shareableDirectory = string.Empty;
            var connectionManagerDatabaseServers = ConfigurationManager.GetSection("ConnectionManagerShareableFolder") as NameValueCollection;
            if (connectionManagerDatabaseServers != null)
            {
                shareableDirectory = connectionManagerDatabaseServers["directory"].ToString();
            }
            // The Following cannot be changed as its needed to support legacy customers
            return new SystemConfiguration() { CompanyDirectoryName = shareableDirectory ?? "Cryptzone" };
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
            return result;
        }

        private static bool AreRequiredArgumentsPresent(Options options)
        {
            const string errorPrefix = "Error: Option --";
            const string errorEmptyValue = " is required argument to run Basic Environment Installer and the value cannot be empty string.";
            bool result = true;
            if (string.IsNullOrWhiteSpace(options.SharedDataDirectory))
            {
                Console.WriteLine($"{errorPrefix}sharedDataDirectory{errorEmptyValue}");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(options.DatabaseServerInstanceName))
            {
                Console.WriteLine($"{errorPrefix}databaseServerInstanceName{errorEmptyValue}");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(options.DatabaseMdfBackupDirectory))
            {
                Console.WriteLine($"{errorPrefix}databaseMdfBackupDirectory{errorEmptyValue}");
                result = false;
            }
            if (string.IsNullOrWhiteSpace(options.ServiceAccountName))
            {
                Console.WriteLine($"{errorPrefix}serviceAccountName{errorEmptyValue}");
                result = false;
            }
            return result;
        }
    }
}
