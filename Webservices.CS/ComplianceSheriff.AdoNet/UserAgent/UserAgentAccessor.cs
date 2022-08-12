using ComplianceSheriff.Configuration;
using ComplianceSheriff.FileSystem;
using ComplianceSheriff.UserAgent;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.UserAgent
{
    public class UserAgentAccessor : IUserAgentAccessor
    {
        private readonly IConnectionManager _connection;
        private readonly ILogger<UserAgentAccessor> _logger;
        private readonly IFileSystemService _fileSystemService;

        public UserAgentAccessor(IConnectionManager connection,
                                 ILogger<UserAgentAccessor> logger,
                                 IFileSystemService fileSystemService)
        {
            _connection = connection;
            _logger = logger;
            _fileSystemService = fileSystemService;
        }

        public async Task<IEnumerable<UserAgentListItem>> GetUserAgentList(string organizationId, CancellationToken cancellationToken)
        {
            var customerFolder = _fileSystemService.GetCustomerFolder(organizationId);
            var userAgentPath = Path.Combine(customerFolder, "data", "UserAgents.txt");

            var userAgentStrings = await _fileSystemService.ReadTextAsync(userAgentPath);

            var userAgentStringArray = userAgentStrings.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            //Add Default value
            var userAgentStringList = new List<UserAgentListItem>
            {
                new UserAgentListItem { Label = "(default)", UserAgentString = string.Empty}
            };

            foreach (var userAgent in userAgentStringArray)
            {
                string[] userAgentParts = userAgent.Split('=');
                userAgentStringList.Add(new UserAgentListItem
                {
                    Label = userAgentParts[0],
                    UserAgentString = userAgentParts[1]
                });
            }

            return userAgentStringList;
        }

        public async Task<UserAgentListItem> GetUserAgentItem(string userAgentLabel, string organizationId, CancellationToken cancellationToken)
        {
            UserAgentListItem userAgentValue = new UserAgentListItem { Label = "(default)", UserAgentString = string.Empty }; // default instance of UserAgent
            var result = await GetUserAgentList(organizationId, cancellationToken);
            foreach (UserAgentListItem uaItem in result)
            {
                if (uaItem.Label.CompareTo(userAgentLabel) == 0)
                {
                    userAgentValue = uaItem;
                    break;
                }
            }
            if (userAgentValue.Label.CompareTo("(default)") == 0)
                _logger.LogInformation("getUserAgent : The UserAgent Label {0} : doesn't exist in the UserAgentList. The default value (empty string) is assigned to User Agent.", userAgentLabel);

            return userAgentValue;
        }
    }
}
