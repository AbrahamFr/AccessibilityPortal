using BasicEnvironment.Database.Setup;
using System;
using System.Collections.Generic;

namespace BasicEnvironment.Setup
{
    public static class SetupSteps
    {
        public static readonly IReadOnlyList<Type> All = new[]
        {
            typeof(MsmqManager),
            typeof(SharedDirectory.FileSystemStorageSetup),
            typeof(SharedDirectory.SqlDefaultDoc),
            typeof(SharedDirectory.UncPathSetup),
            typeof(SqlServicesChecker),
            typeof(DatabaseEnvironment),
        };
    }
}
