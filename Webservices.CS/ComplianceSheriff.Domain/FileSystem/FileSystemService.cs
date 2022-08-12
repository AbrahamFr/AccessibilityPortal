using ComplianceSheriff.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ComplianceSheriff.FileSystem
{
    public class FileSystemService : IFileSystemService
    {
        private readonly ConfigurationOptions _configurationOptions;
        private readonly ILogger<FileSystemService> _logger;

        public FileSystemService(IOptions<ConfigurationOptions> configOptions,
                                 ILogger<FileSystemService> logger)
        {
            _configurationOptions = configOptions.Value;
            _logger = logger;
        }
        public string GetCustomerFolder(string organizationId)
        {
            var directory = $@"\\{_configurationOptions.SharedDir}\Cryptzone\{_configurationOptions.ClusterName}\customers\{organizationId}";
            return directory;
        }

        public async Task<string> ReadTextAsync(string filePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                using (FileStream sourceStream = new FileStream(filePath,
                       FileMode.Open, FileAccess.Read, FileShare.Read,
                       bufferSize: 4096, useAsync: true))
                {                    
                    byte[] buffer = new byte[0x1000];
                    int numRead;
                    while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        string text = Encoding.UTF8.GetString(buffer, 0, numRead);
                        sb.Append(text);
                    }

                    return sb.ToString();
                }
            } 
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
