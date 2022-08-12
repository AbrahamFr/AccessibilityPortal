using ServiceInstaller.Abstractions;
using System.IO;

namespace ServiceInstaller
{
    public static class FileSystemStorageDerivedConfiguration
    {
        public static string GetSharedFullDataDirectory(this IUrnOptions urnOptions, SystemConfiguration configuration)
        {
            string fullPath = urnOptions.ControllerMachineName.Replace(@"/", @"\");
            return Path.Combine(fullPath,
                fullPath.ToLower().IndexOf(@"\\" + configuration.CompanyDirectoryName.ToLower()) == -1
                    ? configuration.CompanyDirectoryName : "",
                fullPath.ToLower().IndexOf(@"\\" + urnOptions.ClusterName.ToLower()) == -1
                    ? urnOptions.ClusterName : ""
                );
        }
    }
}
