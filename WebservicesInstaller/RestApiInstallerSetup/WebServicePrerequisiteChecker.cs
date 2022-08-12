using ComplianceSheriff.Domain.Installers.Abstractions;
using ComplianceSheriff.Domain.Installers.Enums;
using ComplianceSheriff.Domain.Installers.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Web.Administration;
using RestApiInstallerDomain;
using System;
using System.IO;
using System.Linq;


namespace RestApiInstallerSetup
{
    public class WebServicePrerequisiteChecker : ISetupStep
    {
        private readonly IAllOptions serviceOptions;
        private readonly IUrnOptions urnOptions;
        private readonly IMessageFormatter messageFormatter;
        private readonly IFolderOptions folderOptions;
        private readonly SystemConfiguration configuration;
        private readonly ILogger<WebServicePrerequisiteChecker> logger;
        private string _message = string.Empty;

        private Application _webServiceApplication = null;

        public WebServicePrerequisiteChecker(IUrnOptions urnOptions,
                                           IAllOptions serviceOptions,
                                           IFolderOptions folderOptions,
                                           IMessageFormatter messageFormatter,
                                           ILogger<WebServicePrerequisiteChecker> logger,
                                           IOptions<SystemConfiguration> configuration)
        {
            this.logger = logger;
            this.urnOptions = urnOptions;
            this.folderOptions = folderOptions;
            this.serviceOptions = serviceOptions;
            this.configuration = configuration.Value;
            this.messageFormatter = messageFormatter;
        }

        public void Setup()
        {
            //Does application exist
            var applicationExists = CheckApplicationExists(serviceOptions.TargetAppName, urnOptions.ClusterName);

            //Does the DestinationDirectory match existing Web App Path?
            if (applicationExists)
            {
                CompareWebAppPathWithDestinationDirArg(serviceOptions.DestinationDirectory, serviceOptions.TargetAppName);
            }
        }

        private bool CheckApplicationExists(string targetAppName, string clusterName)
        {
            bool result = false;
            Microsoft.Web.Administration.Site _webAppSite = null;

            using (var serverManager = new ServerManager())
            {
                //Get Default Website
                var sites = serverManager.Sites.ToList();

                if (sites.Any(s => s.Name == configuration.DefaultWebSiteName))
                {
                    _webAppSite = serverManager.Sites.ToList().Find(s => s.Name == configuration.DefaultWebSiteName);
                }
            }

            //If Default Website Exists
            if (_webAppSite != null)
            {
                //Check Default Website Applications for WebServices
                if (_webAppSite.Applications.Any())
                {
                    //var webServiceAppName = String.Format("{0}_{1}", clusterName, targetAppName);
                    var webServiceApp = _webAppSite.Applications.Where(app => app.Path.ToLower() == "/" + targetAppName.ToLower()).FirstOrDefault();

                    if (webServiceApp != null)
                    {
                        result = true;

                        //Initializes Application object in order to retrieve physical path of the app 
                        //and compare with destinationDirectory argument passed in by user
                        _webServiceApplication = webServiceApp;
                    }
                }

                _message = messageFormatter.FormatMessage($"Web Application: {targetAppName} Exists: {result}", MessageType.Info);
                logger.LogInformation(_message);

            }
            else
            {
                throw new InvalidOperationException(_message);
            }

            return result;
        }

        private bool CompareWebAppPathWithDestinationDirArg(string destinationDirArg, string targetAppName)
        {
            bool result = false;
            string fullDestinationDirectory = string.Empty;
            var webServiceAppPath = _webServiceApplication.VirtualDirectories[0].PhysicalPath;

            if (_webServiceApplication != null)
            {
                //Compare First 3 segments of Web Application Path with destinationDirectory               
                if (ComparePathSegments(webServiceAppPath, destinationDirArg, 3))
                {
                    fullDestinationDirectory = SharedHelper.BuildFullDestinationPath(serviceOptions, folderOptions, configuration);
                    result = String.Compare(webServiceAppPath.Trim().ToLower(), fullDestinationDirectory.Trim().ToLower()) == 0;
                }
            }

            logger.LogInformation(messageFormatter.FormatMessage($"Compare --destinationDirectoryPath '{fullDestinationDirectory}' with the Current Web Service Application Path: '{webServiceAppPath}'", MessageType.Info));

            if (!result)
            {
                _message = messageFormatter.FormatMessage("The value calculated from argument '--destinationDirectory' and the current Web Application Physical path do not match.", MessageType.Error);
                logger.LogError(_message);

                throw new InvalidOperationException(_message);
            }

            return result;
        }

        private bool ComparePathSegments(string path1, string path2, int segmentsToCompare)
        {
            bool result = true;

            var path1Segments = path1.ToLower().Split('\\');
            var path2Segments = path2.ToLower().Split('\\');

            if (path1Segments.Length >= segmentsToCompare && path2Segments.Length >= segmentsToCompare)
            {
                for (int i = 0; i < segmentsToCompare; i++)
                {
                    if (path1Segments[i].ToString() != path2Segments[i].ToString())
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
