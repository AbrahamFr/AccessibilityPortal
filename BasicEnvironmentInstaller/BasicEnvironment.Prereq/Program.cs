using CommandLine;
using System;
using Microsoft.Win32;
using CommandLine.Text;

namespace BasicEnvironmentPrereq
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.Title = "Compliance Sheriff Basic Environment Prerequisite Checker";
            Console.WriteLine("Looking for .NET Framework...");

            bool hasProperVersion = IsDotNetFrameworkInstalled();
            if (hasProperVersion)
            {
                Console.WriteLine("Basic Environment Prerequisite Checker");

                var result = new Parser(with => with.IgnoreUnknownArguments = true)
                    .ParseArguments<Options>(args);

                return result.MapResult(
                    parsedFunc: options =>
                    {
                        Console.WriteLine("Arguments have been read into Basic Environment Prerequisite");
                        if (!options.Quiet)
                        {
                            Console.WriteLine("The .NET Framework is installed.");
                        }
                        return new EnvironmentSetup(options).Setup(args);
                     
                    }, 
                    notParsedFunc: errors =>
                    {
                        Console.WriteLine(HelpText.AutoBuild(result).ToString());
                        return 1;
                    });
            }
            else
            {
                Console.WriteLine("Please install the .NET Framework Version 4.7.2 or later");
                Console.ReadLine();
                return 1;
            }
        }

        #region Verify Dot NET Framework

        private static bool IsDotNetFrameworkInstalled()
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
