using System;
using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Scans
{
    public class ScanUpdateRequest : ScanRequest
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        [RegularExpression("([0-9]+)", ErrorMessage = "{0}MustBeInteger")]       
        public string ScanId { get { return scanId; }
                               set { scanId = value; }
        }

        public static explicit operator Scan(ScanUpdateRequest scanRequest)
        {
            var scan = new Scan
            {
                ScanId = Int32.TryParse(scanRequest.ScanId, out int outScanId) ? outScanId : 0,
                IsMonitor = scanRequest.IsMonitor,
                BaseUrl = scanRequest.BaseUrl,
                DisplayName = scanRequest.DisplayName,
                CheckpointGroupIds = scanRequest.CheckpointGroupIds,
                IncludedDomains = scanRequest.IncludedDomains,
                Path = scanRequest.Path,
                Levels = scanRequest.Levels,
                Script = scanRequest.Script,
                UserName = scanRequest.UserName,
                Password = scanRequest.Password,
                Domain = scanRequest.Domain,
                PageLimit = scanRequest.PageLimit,
                WindowWidth = scanRequest.WindowWidth,
                WindowHeight = scanRequest.WindowHeight,
                IncludeFilter = scanRequest.IncludeFilter,
                ExcludeFilter = scanRequest.ExcludeFilter,
                UserAgent = scanRequest.UserAgent,
                IncludeMsofficeDocs = Boolean.TryParse(scanRequest.IncludeMsofficeDocs, out bool includeMSOfficeDocs) ? includeMSOfficeDocs : false,
                IncludeOtherDocs = Boolean.TryParse(scanRequest.IncludeOtherDocs, out bool includeOtherDocs) ? includeOtherDocs : false,
                RetestAllPages = Boolean.TryParse(scanRequest.RetestAllPages, out bool retestAllPages) ? retestAllPages : false,
                AlertMode = scanRequest.AlertMode,
                AlertDelay = scanRequest.AlertDelay,
                AlertSendTo = scanRequest.AlertSendTo,
                AlertSubject = scanRequest.AlertSubject,
                LocalClientId = scanRequest.LocalClientId,
                StartPages = scanRequest.StartPages,
                DateCreated = scanRequest.DateCreated,
                DateModified = scanRequest.DateModified
            };

            return scan;
        }
    }
}
