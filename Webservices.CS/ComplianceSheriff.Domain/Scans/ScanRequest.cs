using ComplianceSheriff.TextFormatter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;

namespace ComplianceSheriff.Scans
{
    public class ScanRequest : IValidatableObject
    {
        protected string scanId;

        [JsonIgnore]
        public bool IsMonitor { get; set; } = false;

        [Required(ErrorMessage = "{0}IsRequired")]
        public string BaseUrl { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "CheckpointGroupIdIsRequired")]
        public List<string> CheckpointGroupIds { get; set; }

        public string IncludedDomains { get; set; }

        #region "StartPages"

        [Required(ErrorMessage = "{0}IsRequired")]
        public string Path { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string Levels { get; set; }

        public string Script { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Domain { get; set; } = string.Empty;

        [Required(ErrorMessage = "PageLimitIsRequired")]
        public string PageLimit { get; set; }
        public int WindowWidth { get; set; } = 0;
        public int WindowHeight { get; set; } = 0;

        #endregion

        public string IncludeFilter { get; set; } = string.Empty;

        public string ExcludeFilter { get; set; } = string.Empty;

        public string UserAgent { get; set; } = string.Empty;

        public string IncludeMsofficeDocs { get; set; }

        public string IncludeOtherDocs { get; set; }

        public string RetestAllPages { get; set; }

        //Note:-- Below properties are for monitor may not required and set to default values. 
        [JsonIgnore]
        public int AlertMode { get; set; } = 3;  // 3 = AlertMode.WhenResultChanges
        [JsonIgnore]
        public int AlertDelay { get; set; } = 1;
        [JsonIgnore]
        public string AlertSendTo { get; set; } = string.Empty;
        [JsonIgnore]
        public string AlertSubject { get; set; } = string.Empty;

        public string LocalClientId { get; set; } = string.Empty;
        [JsonIgnore]
        public List<StartPage> StartPages { get; set; }
        [JsonIgnore]
        public DateTimeOffset? DateCreated { get; set; }
        [JsonIgnore]
        public DateTimeOffset? DateModified { get; set; }

        public StartPage CreateStartPages()
        {
            StartPage requestStartPage = new StartPage
            {
                Path = Path,
                PageLimit = PageLimit,
                Levels = Levels,
                Script = Script,
                Username = UserName,
                Password = Password,
                Domain = Domain,
                WindowHeight = 0,
                WindowWidth = 0
            };
            return requestStartPage;
        }

        public int ScanGroupId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            Uri uriResult = null;

            if (!String.IsNullOrWhiteSpace(scanId))
            {
                bool result = Int32.TryParse(scanId, out int number);
                if (!result)
                {
                    yield return new ValidationResult("scanIdMustBeInteger");
                }
            }

            if (CheckpointGroupIds != null)
            {
                if (CheckpointGroupIds.Count < 1)
                    yield return new ValidationResult("PleaseEnterCheckpointGroupIdDetails");
                else
                {
                    foreach (string chkGroupId in CheckpointGroupIds)
                        if (string.IsNullOrEmpty(chkGroupId))
                            yield return new ValidationResult("PleaseEnterValidCheckpointGroupId");
                }
            }
            else if (CheckpointGroupIds == null)
                yield return new ValidationResult("PleaseEnterCheckpointGroupIdInformation");

            if (!(Uri.TryCreate(BaseUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)))
                yield return new ValidationResult("BaseUrlInvalidHttpSchemeNotFound");
            else
            {
                Uri startUrl = new Uri(BaseUrl);
                bool isHostResolve = true;
                if (startUrl.Scheme == Uri.UriSchemeHttp || startUrl.Scheme == Uri.UriSchemeHttps)
                {
                    HttpWebRequest request = WebRequest.Create(BaseUrl) as HttpWebRequest;
                    try
                    {
                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    }
                    catch(WebException) // Added due to request throwing an exception on some SSL Sites
                    {
                    }
                    catch(Exception) //If exception thrown that means couldn't get response from address
                    {
                        isHostResolve = false;
                    }
                    if (!isHostResolve)
                        yield return new ValidationResult("BaseUrlInvalidWithFormatOrHost");
                }
                else
                    yield return new ValidationResult("BaseUrlInvalid");
            }

            if (string.IsNullOrEmpty(DisplayName))
            {
                yield return new ValidationResult("EnterValidScanDisplayName");
            }
            if (!string.IsNullOrWhiteSpace(IncludedDomains))
            {
                string[] domainLists = IncludedDomains.Split(TextFormatterService.includedDomainsSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string domain in domainLists)
                {
                    if (Uri.CheckHostName(domain) == UriHostNameType.Unknown)
                        yield return new ValidationResult("InvalidIncludedDomain");
                }
            }
            if (string.IsNullOrEmpty(PageLimit))
                yield return new ValidationResult("PageLimitAlwaysMoreThanZero");
            else
            {
                bool success = Int32.TryParse(PageLimit, out int number);
                if (success)
                {
                    if ((Enumerable.Range(0, 1).Contains(number)) || (number < 0))
                        yield return new ValidationResult("PageLimitAlwaysMoreThanZero");
                }
                else
                    yield return new ValidationResult("EnterValidValuesForPageLimit");
            }

            if (string.IsNullOrEmpty(Levels))
                yield return new ValidationResult("PageLevelMustBeBetween0And20");
            else
            {
                bool success = Int32.TryParse(Levels, out int result);
                if (success)
                {
                    if (!Enumerable.Range(0, 21).Contains(result))
                        yield return new ValidationResult("PageLevelMustBeBetween0And20");
                }
                else
                    yield return new ValidationResult("EnterValidValuesForLevels");
            }

            if (string.IsNullOrEmpty(IncludeMsofficeDocs))
                IncludeMsofficeDocs = "false";
            else
            {
                bool success = Boolean.TryParse(IncludeMsofficeDocs, out bool result);
                if (success)
                {
                    IncludeMsofficeDocs = result.ToString();
                }
                else
                    yield return new ValidationResult("invalidValueForIncludeMsofficeDocs");
            }

            if (string.IsNullOrEmpty(IncludeOtherDocs))
                IncludeOtherDocs = "false";
            else
            {
                bool success = Boolean.TryParse(IncludeOtherDocs, out bool result);
                if (success)
                {
                    IncludeOtherDocs = result.ToString();
                }
                else
                    yield return new ValidationResult("invalidValueForIncludeOtherDocs");
            }

            if (string.IsNullOrEmpty(RetestAllPages))
                RetestAllPages = "false";
            else
            {
                bool success = Boolean.TryParse(RetestAllPages, out bool result);
                if (success)
                {
                    RetestAllPages = result.ToString();
                }
                else
                    yield return new ValidationResult("invalidValueForRetestAllPages");
            }
        }
    }
}
