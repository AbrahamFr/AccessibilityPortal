
using IISPrerequisities;
using MSMQPrerequisites;
using SharedDataDirectoryPrerequisites;
using System;
using System.Collections.Generic;

namespace RestApiInstallerSetup
{
    public static class SetupSteps
    {
        public static readonly IReadOnlyList<Type> All = new[]
         {
            typeof(MSMQPrerequisiteChecker),
            typeof(SharedDataDirectoryChecker),
            typeof(IISPrerequisiteChecker),
            typeof(WebServicePrerequisiteChecker),
            typeof(WebServiceInstaller),
            typeof(WindowsService),
            typeof(WinServiceFolder)
        };
    }
}
