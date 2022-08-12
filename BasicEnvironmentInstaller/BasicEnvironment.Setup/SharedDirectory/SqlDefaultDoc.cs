using BasicEnvironment.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace BasicEnvironment.Setup.SharedDirectory
{
    [System.ComponentModel.Description("Database Configuration")]
    internal class SqlDefaultDoc : ISetupStep
    {
        private readonly string defaultFilePath;
        private readonly IClusterDataOptions clusterOptions;
        private readonly ILogger<SqlDefaultDoc> logger;

        public SqlDefaultDoc(
            IClusterDataDirectoryOptions sharedDirectoryOptions,
            IClusterDataOptions clusterOptions,
            ILogger<SqlDefaultDoc> logger,
            SystemConfiguration configuration)
        {
            this.defaultFilePath = $"{sharedDirectoryOptions.GetSharedFullDataDirectory(configuration)}\\config\\sqlserver\\default.xml";
            this.clusterOptions = clusterOptions;
            this.logger = logger;
        }

        public void Setup()
        {
            logger.LogInformation($"File to create: {defaultFilePath}");
            if (!File.Exists(defaultFilePath))
            {
                // Create a file to write to.
                using (var fs = File.Create(defaultFilePath))
                {
                    logger.LogInformation("Created 'default.xml'");
                }

            }
            var defaultText = File.OpenText(defaultFilePath);
            var fileLength = defaultText.ReadToEnd().Length;
            defaultText.Close();
            logger.LogInformation($"default.xml file length is {fileLength}");
            if (fileLength > 0)
            {
                var defaultDoc = XDocument.Load(defaultFilePath, LoadOptions.PreserveWhitespace);

                using (var resourceStream = this.GetType().Assembly.GetManifestResourceStream($"{this.GetType().Namespace}.Resources.DatabaseConfig.xsd"))
                using (var xsdStreamReader = new StreamReader(resourceStream))
                using (var xsdXmlReader = XmlReader.Create(xsdStreamReader))
                {
                    XmlSchemaSet schemas = new XmlSchemaSet();
                    schemas.Add("", xsdXmlReader);
                    defaultDoc.Validate(schemas, (o, e) =>
                    {
                        throw new XmlException($@"Error in file {defaultFilePath}
{e.Message}
If Host is missing, either delete file and run again or manually fix node.
Example:
{GenerateDefaultDatabaseConfig()}");
                    });
                    var hostElement = defaultDoc.Root.Elements().First(el => el.Name == "Host");
                    if (hostElement.IsEmpty || String.IsNullOrWhiteSpace(hostElement.Value))
                    {
                        hostElement.SetValue(clusterOptions.DatabaseHost);
                        defaultDoc.Save(defaultFilePath);
                    }
                    else
                    {
                        if (hostElement.Value.ToUpper() != clusterOptions.DatabaseHost.ToUpper())
                        {
                            throw new XmlException($"Error in file {defaultFilePath}: Host should be { clusterOptions.DatabaseHost }, instead is {hostElement.Value}");
                        }
                    }
                }
            }
            else
            {
                var sb = new StringBuilder();
                var xws = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true
                };

                using (StreamWriter writer = File.CreateText(defaultFilePath))
                {
                    writer.Write(GenerateDefaultDatabaseConfig());
                }
                logger.LogInformation("default.xml updated");
            }

        }

        private string GenerateDefaultDatabaseConfig()
        {
            // TODO - load XDocument from embedded resource via XDocument.ReadFrom, then mutate the Host element via XPath
            return $@"<Configuration>
  <Host>{clusterOptions.DatabaseHost}</Host>
  <!--ComplianceSheriff Supports following Parameters-->
  <!--<MaxPoolSize>300</MaxPoolSize>-->
  <!--<ConnectTimeOut>200</ConnectTimeOut>-->
  <!--for adding additional parameters to connection string, Please refer https://www.codeproject.com/articles/17768/ado-net-connection-pooling-at-a-glance-->
  <!--Make sure that you don't specify MaxPoolSize and ConnectTimeOut in Additional parameters-->
  <!--<AdditionalParameters>Pooling=True;Min Pool Size=100;</AdditionalParameters>-->
</Configuration>";
        }
    }
}
