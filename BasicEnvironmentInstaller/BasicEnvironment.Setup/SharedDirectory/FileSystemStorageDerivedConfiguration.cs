using BasicEnvironment.Abstractions;
using System.IO;

namespace BasicEnvironment.Setup.SharedDirectory
{
    public static class FileSystemStorageDerivedConfiguration
    {
        public static string GetSharedFullDataDirectory(this IClusterDataDirectoryOptions clusterOptions, SystemConfiguration configuration)
        {
            string fullPath = clusterOptions.SharedDataDirectory.Replace(@"/", @"\");
            return Path.Combine(fullPath,
                fullPath.ToLower().IndexOf(configuration.CompanyDirectoryName.ToLower()) == -1
                    ? configuration.CompanyDirectoryName : "",
                fullPath.ToLower().Substring(fullPath.LastIndexOf('\\')).IndexOf(clusterOptions.ClusterName.ToLower()) == -1
                    ? clusterOptions.ClusterName : ""
                );
        }
    }
}
