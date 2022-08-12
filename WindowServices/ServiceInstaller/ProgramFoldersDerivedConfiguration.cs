using ServiceInstaller.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServiceInstaller
{
    public static class ProgramFoldersDerivedConfiguration
    {
        public static string GetFullDestinationDirectory(this IFolderOptions folderOptions, SystemConfiguration configuration)
        {
            string fullPath = folderOptions.DestinationDirectory.Replace(@"/", @"\");
            return Path.Combine(fullPath,
                fullPath.ToLower().IndexOf(configuration.CompanyDirectoryName.ToLower()) == -1
                    ? configuration.CompanyDirectoryName : "",
                fullPath.ToLower().IndexOf("Compliance Sheriff".ToLower()) == -1
                    ? "Compliance Sheriff" : "",
                fullPath.ToLower().IndexOf(folderOptions.ServiceType.ToLower()) == -1
                    ? folderOptions.ServiceType.ToLower() : ""
                );
        }
    }
}
