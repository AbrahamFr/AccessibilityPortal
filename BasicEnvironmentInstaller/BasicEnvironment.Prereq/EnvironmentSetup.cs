using System;
using System.Diagnostics;

namespace BasicEnvironmentPrereq
{
    internal class EnvironmentSetup
    {
        private Options options;

        public EnvironmentSetup(Options options)
        {
            this.options = options;
        }

        internal int Setup(string[] args)
        {
            string processName = $"{options.SourceDirectory}\\BasicEnvironment.Setup.exe";
            Console.WriteLine($"Attempting to start process in {processName}");

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = processName;
                for (int i = 0; i < args.Length; i++)
                {

                    if (!args[i].StartsWith("--sourceDirectory"))
                    {
                        p.StartInfo.ArgumentList.Add(args[i]);
                    }
                }
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = false;

                Console.WriteLine($"Invoking {processName}");
                p.Start();

                p.WaitForExit();
                return p.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}
