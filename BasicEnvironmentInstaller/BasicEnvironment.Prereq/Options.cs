using CommandLine;

namespace BasicEnvironmentPrereq
{
    public class Options
    {
        [Option('q', "quiet", Required = false, HelpText = "Run without prompts to the user.")]
        public bool Quiet { get; set; }
        [Option("sourceDirectory", Default = "..\\CS_BasicEnvironmentSetup", HelpText = "Directory with the BasicEnvironment.exe application.")]
        public string SourceDirectory { get; set; }
    }
}
