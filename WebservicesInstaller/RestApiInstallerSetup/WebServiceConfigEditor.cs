using ConfigurationFiles;
using Microsoft.Extensions.Logging;
using System.IO;

namespace RestApiInstallerSetup
{
    public class WebServiceConfigEditor
    {
        private readonly ILogger<WebServiceConfigEditor> _logger;
        private readonly DotNetFrameworkConfigEditor webConfigEditor;

        public WebServiceConfigEditor(ILogger<WebServiceConfigEditor> logger,
                                      DotNetFrameworkConfigEditor webConfigEditor)
        {
            _logger = logger;
            this.webConfigEditor = webConfigEditor;
        }

        public void IncreaseRequestLimits(string webAppDir, int maxAllowedContentLength)
        {
            _logger.LogInformation("Setting Request Limits on Web Service");

            var webConfigPath = Path.Combine(webAppDir, "web.config");
            var doc = webConfigEditor.LoadConfiguration(webConfigPath);

            var configurationElement = doc.DocumentElement;

            if (doc.SelectSingleNode($"system.webServer") == null)
            {
                //create node and add value
                var webServerElement = doc.CreateElement("system.webServer", null);
                var securityElement = doc.CreateElement("security", null);
                var requestFilteringElement = doc.CreateElement("requestFiltering", null);
                var requestLimitsElement = doc.CreateElement("requestLimits", null);

                requestLimitsElement.SetAttribute("maxAllowedContentLength", maxAllowedContentLength.ToString());

                requestFilteringElement.AppendChild(requestLimitsElement);
                securityElement.AppendChild(requestFilteringElement);
                webServerElement.AppendChild(securityElement);

                configurationElement.AppendChild(webServerElement);

                webConfigEditor.SaveConfiguration(doc, webConfigPath);
            }
        }
    }
}
