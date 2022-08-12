using ComplianceSheriff.Domain.Installers.Abstractions;
using ConfigurationFiles;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace RestApiInstallerSetup
{
    public class JwtSigningKeyConfigEditor
    {
        public readonly ILogger<JwtSigningKeyConfigEditor> logger;
        private readonly JsonConfigEditor jsonConfigEditor;
        private readonly IClusterSettings clusterSettings;
        private readonly IAllOptions allOptions;

        public JwtSigningKeyConfigEditor(ILogger<JwtSigningKeyConfigEditor> logger,
                                JsonConfigEditor jsonConfigEditor,
                                IClusterSettings clusterSettings,
                                IAllOptions allOptions)
        {
            this.logger = logger;
            this.jsonConfigEditor = jsonConfigEditor;
            this.clusterSettings = clusterSettings;
            this.allOptions = allOptions;
        }

        public void SetJwtSigningKey(string configPath)
        {
            //Set authentication.json path
            var jsonFilePath = Path.Combine(configPath, "authentication.json");
            var jsonObject = jsonConfigEditor.LoadConfiguration(jsonFilePath);           

            var currentSecretKey = jsonObject["JwtSigningKey"].Value<string>();

            string jsonResult = string.Empty;

            //If value is empty, then retrieve Secret key from database
            if (String.IsNullOrWhiteSpace(currentSecretKey))
            {
                logger.LogInformation($"Setting JWT Secret Key in {allOptions.TargetAppName} Web Service");

                var jwtSigningKey = RetrieveSecretKey();

                if (!String.IsNullOrWhiteSpace(jwtSigningKey))
                {
                    jsonObject["JwtSigningKey"] = jwtSigningKey;
                    jsonResult = jsonObject.ToString();
                }
                else
                {
                    throw new ApplicationException("JwtSigningKey is not set.");
                }

                jsonConfigEditor.SaveConfiguration(jsonFilePath, jsonResult);
            }            
        }

        private string RetrieveSecretKey()
        {
            logger.LogInformation("Retrieving Secret Key");
            var signingKey = clusterSettings.GetClusterSettings("_main", "JwtSigningKey");
            return signingKey;
        }
    }
}
