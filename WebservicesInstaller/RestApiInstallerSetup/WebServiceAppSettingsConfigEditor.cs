using ComplianceSheriff.Domain.Installers.Abstractions;
using ComplianceSheriff.Domain.Installers.Extensions;
using ConfigurationFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Xml;

namespace RestApiInstallerSetup
{
    public class WebServiceAppSettingsConfigEditor
    {
        private readonly ILogger<WebServiceAppSettingsConfigEditor> logger;
        private readonly JsonConfigEditor jsonConfigEditor;
        private readonly IClusterSettings clusterSettings;
        private readonly SystemConfiguration configuration;
        private readonly IUrnOptions urnOptions;
        private readonly IAllOptions allOptions;

        public WebServiceAppSettingsConfigEditor(ILogger<WebServiceAppSettingsConfigEditor> logger,
                                                 JsonConfigEditor jsonConfigEditor,
                                                 IClusterSettings clusterSettings,
                                                 IUrnOptions urnOptions,
                                                 IOptions<SystemConfiguration> configuration,
                                                 IAllOptions allOptions)
        {
            this.logger = logger;
            this.jsonConfigEditor = jsonConfigEditor;
            this.clusterSettings = clusterSettings;
            this.configuration = configuration.Value;
            this.urnOptions = urnOptions;
            this.allOptions = allOptions;
        }

        public void SetAppSettings(string configPath)
        {
            string result = string.Empty;          
            var jsonFilePath = Path.Combine(configPath, "appSettings.json");
            var jsonObject = jsonConfigEditor.LoadConfiguration(jsonFilePath);

            jsonObject["Shared"]["SharedDir"] = allOptions.ControllerMachineName;
            jsonObject["Shared"]["ClusterName"] = allOptions.ClusterName;
            jsonObject["QueueServerBaseUrl"] = clusterSettings.GetClusterSettings("_main", "QueueServerBaseUrl");
            jsonObject["JWTExpirationInMinutes"] = 5;


            var dbConfigFilePath = Path.Combine("\\\\",urnOptions.GetSharedFullDataDirectory(configuration), "config", "sqlserver", "default.xml");

            var doc = new XmlDocument();
            doc.Load(dbConfigFilePath);
            XmlNode dbNode = doc.SelectSingleNode("Configuration/Host");

            if (jsonObject["APILoggerConnection"] == null)
            {
                jsonObject["APILoggerConnection"] = String.Format("Server={0};Database={1}_main;Trusted_Connection=True;", dbNode.InnerText, allOptions.ClusterName);
            }              

            result = jsonObject.ToString();

            logger.LogInformation("Configuring Application Settings...");
            jsonConfigEditor.SaveConfiguration(jsonFilePath, result);
        }
    }
}
