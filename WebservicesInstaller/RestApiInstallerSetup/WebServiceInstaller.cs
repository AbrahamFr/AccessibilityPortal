using ComplianceSheriff.Domain.Installers.Abstractions;
using ComplianceSheriff.Domain.Installers.Extensions;
using ConfigurationFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestApiInstallerDomain;
using System;
using System.IO;

namespace RestApiInstallerSetup
{
    public class WebServiceInstaller : ISetupStep
    {
        private readonly IAllOptions _options;
        private readonly IUrnOptions _urnOptions;
        private readonly IFolderOptions _folderOptions;
        private readonly SystemConfiguration _configuration;
        private readonly IClusterSettings _clusterSettings;
        private readonly ILogger<WebServiceInstaller> _logger;
        private readonly IFileSystemManager _fileSystemManager;
        private readonly IFileSecurityManager _fileSecurityManager;
        private readonly IWebServerManager _webServerManager;
        private readonly ISystemAccountManager _systemAccountManager;
        private readonly WebServiceConfigEditor _webServiceConfigEditor;
        private readonly JwtSigningKeyConfigEditor _jwtSigningKeyConfigEditor;
        private readonly WebServiceAppSettingsConfigEditor _webServiceAppSettingsConfigEditor;     

        public WebServiceInstaller(IAllOptions options,
                                   IUrnOptions urnOptions,
                                   IFolderOptions folderOptions,
                                   IClusterSettings clusterSettings,
                                   IFileSystemManager fileSystemManager,
                                   ISystemAccountManager systemAccountManager,
                                   IFileSecurityManager fileSecurityManager,
                                   IWebServerManager webServerManager,
                                   IOptions<SystemConfiguration> configuration,
                                   WebServiceConfigEditor webServiceConfigEditor,
                                   JwtSigningKeyConfigEditor jwtSigningKeyConfigEditor,
                                   DotNetFrameworkConfigEditor dotNetFrameworkConfigEditor,
                                   WebServiceAppSettingsConfigEditor webServiceAppSettingsConfigEditor,
                                   ILogger<WebServiceInstaller> logger)
        {
            _options = options;
            _urnOptions = urnOptions;            
            _folderOptions = folderOptions;
            _clusterSettings = clusterSettings;
            _configuration = configuration.Value;
            _fileSystemManager = fileSystemManager;
            _fileSecurityManager = fileSecurityManager;
            _webServerManager = webServerManager;
            _systemAccountManager = systemAccountManager;
            _webServiceConfigEditor = webServiceConfigEditor;
            _jwtSigningKeyConfigEditor = jwtSigningKeyConfigEditor;
            _webServiceAppSettingsConfigEditor = webServiceAppSettingsConfigEditor;
            _logger = logger;
        }

        public void Setup()
        {
            //COMMON VARIABLES        
            var appDirPath = SharedHelper.BuildFullDestinationPath(_options, _folderOptions, _configuration);
            var appPoolName = String.Format("{0}_{1}", _options.ClusterName, _options.TargetAppName);

            var appPoolIdentity = new AppPoolIdentity
            {
                UserName = _options.ServiceAccountName,
                Password = _options.ServiceAccountPassword
            };

            //CREATE APPLICATION DIRECTORY
            _fileSystemManager.CreateFolder(appDirPath);
            
            //CREATE APP_POOL
            CreateAppPool(_options, appPoolName, appPoolIdentity);

            //CREATE WEB APPLICATION
            CreateApplication(_options.TargetAppName, appPoolName, appDirPath);

            //STOP WEB APP POOL
            StopWebAppPool(appPoolName);

            //REMOVE WEB APPLICATION FILES
            _fileSystemManager.RemoveContent(appDirPath, new string[1] { "authentication.json" });


            //DEPLOY FILES
            _fileSystemManager.DeployFiles(_options.SourceDirectory, appDirPath);

            ConfigureAppSettings(appDirPath);

            SetDirectoryPermissions(appPoolIdentity, appDirPath);

            SetSecretKey(appDirPath);

            UpdateWebConfig();

            //START WEB APP POOL
            StartWebAppPool(appPoolName);

            var webServiceInstallCompleteMsg = String.Format("{0}{1}{2}{1}{0}", "***************************************", Environment.NewLine, "WebService Installation Successful !!!");
            _logger.LogInformation(webServiceInstallCompleteMsg);
        }

        private void CreateAppPool(IAllOptions options, string appPoolName, AppPoolIdentity appPoolIdentity)
        {
            _webServerManager.CreateAppPool(appPoolName, appPoolIdentity.UserName, appPoolIdentity.Password);
        }

        private void CreateApplication(string targetAppName, string appPoolName, string appDirectoryPath)
        {
            _webServerManager.CreateWebApplication(targetAppName, appPoolName, appDirectoryPath);
        }

        private void ConfigureAppSettings(string appDirectoryPath)
        {
            string result = string.Empty;
            var jsonFilePath = Path.Combine(appDirectoryPath, "appSettings.json");
            
            try
            {
                _webServiceAppSettingsConfigEditor.SetAppSettings(appDirectoryPath);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                Environment.Exit(ex.HResult);
            }
        }

        private void StopWebAppPool(string appPoolName)
        {
            try
            {
                _webServerManager.StopWebAppPool(appPoolName);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                Environment.Exit(ex.HResult);
            }
        }

        private void StartWebAppPool(string appPoolName)
        {
            try
            {
                _webServerManager.StartWebAppPool(appPoolName);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                Environment.Exit(ex.HResult);
            }
        }

        private void SetSecretKey(string jsonFilePath)
        {
            if (!String.IsNullOrWhiteSpace(jsonFilePath))
            {
                try
                {
                    _jwtSigningKeyConfigEditor.SetJwtSigningKey(jsonFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.Message);
                    Environment.Exit(ex.HResult);
                } 
            }
        }

        private void SetDirectoryPermissions(AppPoolIdentity identity, string appDirPath)
        {
            _logger.LogInformation("Setting Security Permissions");

            var serviceAccountSecId = _systemAccountManager.GetNtAccount(identity.UserName);
            var adminsSecId = _systemAccountManager.GetAdminAccounts();
            _fileSecurityManager.SetPermissions(serviceAccountSecId, adminsSecId, appDirPath);
        }

        private void UpdateWebConfig()
        {
            _logger.LogInformation("Updating web.config Settings...");

            try
            {
                var configFilePath = SharedHelper.BuildFullDestinationPath(_options, _folderOptions, _configuration);
                _webServiceConfigEditor.IncreaseRequestLimits(configFilePath, 105000000);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                Environment.Exit(ex.HResult);
            }
        }

    }
}
