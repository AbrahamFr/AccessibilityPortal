using ComplianceSheriff.Domain.Installers.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestApiInstallerDomain;
using System;
using System.Data.SqlClient;

namespace RestApiInstallerSetup
{
    public class ClusterSettings : IClusterSettings
    {
        private readonly IAllOptions _allOptions;
        private readonly IUrnOptions _urnOptions;
        private readonly IFolderOptions _folderOptions;
        private readonly ILogger<ClusterSettings> _logger;

        private readonly SystemConfiguration _configuration;

        public ClusterSettings(IAllOptions allOptions,
                               IUrnOptions urnOptions,
                               ILogger<ClusterSettings> logger,
                               IFolderOptions folderOptions,
                               IOptions<SystemConfiguration> configuration)
        {
            _allOptions = allOptions;
            _urnOptions = urnOptions;
            _logger = logger;
            _folderOptions = folderOptions;
            _configuration = configuration.Value;            
        }

        public string GetClusterSettings(string dbName, string clusterSettingsKey)
        {
            string rtrnval = String.Empty;
            var connectionString = SharedHelper.BuildDBConnectionString(dbName, _allOptions, _urnOptions, _configuration);

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand($"Select * FROM ClusterSettings Where SettingsKey = '{clusterSettingsKey}'", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    rtrnval = reader["SettingsValue"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return rtrnval;
        }
    }
}
