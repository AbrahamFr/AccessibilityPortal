using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceInstaller
{
    public static class SetupSteps
    {
        public static readonly IReadOnlyList<Type> All = new[]
        {
            typeof(EnsureBasicEnvironment),
            typeof(MsmqManager),
            typeof(WindowsService),
            typeof(ProgramFolder)
        };
    }
}
