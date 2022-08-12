using ComplianceSheriff.Domain.Installers.Abstractions;
using ConfigurationFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestApiInstallerDomain;
using System;
using System.IO;
using System.Xml;

namespace RestApiInstallerSetup
{
    public class QueueServiceConfigEditor
    {
        private readonly IAllOptions _allOptions;        
        private readonly IUrnOptions _urnOptions;
        private readonly IClusterSettings _clusterSettings;
        private readonly IFolderOptions _folderOptions;
        private readonly SystemConfiguration _configuration;
        private readonly ILogger<QueueServiceConfigEditor> _logger;
        private readonly DotNetFrameworkConfigEditor _dotNetFrameworkConfigEditor;

        public QueueServiceConfigEditor(IAllOptions allOptions,
                                        IUrnOptions urnOptions,
                                        IFolderOptions folderOptions,
                                        IClusterSettings clusterSettings,
                                        IOptions<SystemConfiguration> configuration,
                                        DotNetFrameworkConfigEditor dotNetFrameworkConfigEditor,
                                        ILogger<QueueServiceConfigEditor> logger)
        {
            _allOptions = allOptions;
            _urnOptions = urnOptions;
            _folderOptions = folderOptions;
            _clusterSettings = clusterSettings;
            _configuration = configuration.Value;
            _dotNetFrameworkConfigEditor = dotNetFrameworkConfigEditor;
            _logger = logger;
        }


        public void SetQueueServerConfiguration(string webAppDir)
        {
            var webConfigPath = Path.Combine(webAppDir, $"{_configuration.QueueServiceName}.exe.config");

            try
            {
                _logger.LogInformation("Setting Queue Server appSettings values");
                
                var doc = _dotNetFrameworkConfigEditor.LoadConfiguration(webConfigPath, true);
                XmlNode _appSettingsNode = doc.SelectSingleNode("configuration/appSettings");

                //CloudURI element
                if (_appSettingsNode.SelectSingleNode($"add[@key='CloudURI']") == null)
                {                    
                    var cloudURIElement = doc.CreateElement("add");

                    var cloudURIkeyAttribute = doc.CreateAttribute("key");
                    cloudURIkeyAttribute.InnerText = "CloudURI";

                    var hiCloudUri = new string[4];
                    hiCloudUri[0] = "urn";
                    hiCloudUri[1] = "local";
                    hiCloudUri[2] = _allOptions.ClusterName;
                    hiCloudUri[3] = _allOptions.ControllerMachineName;

                    var cloudURIvalueAttribute = doc.CreateAttribute("value");
                    cloudURIvalueAttribute.InnerText = string.Join(":", hiCloudUri);

                    cloudURIElement.Attributes.Prepend(cloudURIkeyAttribute);
                    cloudURIElement.Attributes.Append(cloudURIvalueAttribute);

                    _appSettingsNode.AppendChild(cloudURIElement);
                }

                //QueueServerBaseUrl element
                if (_appSettingsNode.SelectSingleNode($"add[@key='QueueServerBaseUrl']") == null)
                {
                    var baseUrlElement = doc.CreateElement("add");

                    var baseURLkeyAttribute = doc.CreateAttribute("key");
                    baseURLkeyAttribute.InnerText = "QueueServerBaseUrl";

                    var baseURLvalueAttribute = doc.CreateAttribute("value");
                    baseURLvalueAttribute.InnerText = GetQueueServerBaseUrl();

                    baseUrlElement.Attributes.Prepend(baseURLkeyAttribute);
                    baseUrlElement.Attributes.Append(baseURLvalueAttribute);

                    _appSettingsNode.AppendChild(baseUrlElement);
                }

                //MetaDBConnection element
                if (_appSettingsNode.SelectSingleNode($"add[@key='MetaDBConnection']") == null)
                {
                    var metaConnectionElement = doc.CreateElement("add");

                    var metaKeyAttribute = doc.CreateAttribute("key");
                    metaKeyAttribute.InnerText = "MetaDBConnection";

                    var metaValueAttribute = doc.CreateAttribute("value");
                    metaValueAttribute.InnerText = SharedHelper.BuildDBConnectionString("_meta", _allOptions, _urnOptions, _configuration);

                    metaConnectionElement.Attributes.Prepend(metaKeyAttribute);
                    metaConnectionElement.Attributes.Append(metaValueAttribute);

                    _appSettingsNode.AppendChild(metaConnectionElement);
                }

                _dotNetFrameworkConfigEditor.SaveConfiguration(doc, webConfigPath);
            } 
            catch(Exception ex)
            {
                _logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
                throw new InvalidOperationException($"There was an error setting the QueueServer config settings at {webConfigPath}");
            }
        }

        private string GetQueueServerBaseUrl()
        {
            var baseUrl = _clusterSettings.GetClusterSettings("_main", "QueueServerBaseUrl");
            return baseUrl;
        }

    }
}
