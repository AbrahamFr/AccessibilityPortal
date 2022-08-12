using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ComplianceSheriff.Extensions;
using Newtonsoft.Json;

namespace ComplianceSheriff.Scans
{
    public partial class Scan
    {
        public int ScanId { get; set; }

        public bool IsMonitor { get; set; }

        public string BaseUrl { get; set; }
        public string DisplayName { get; set; }
        public List<string> CheckpointGroupIds { get; set; }    

        #region "StartPages"
        public string Path { get; set; } = string.Empty;

        public string Levels { get; set; } = "0";

        public string Script { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Domain { get; set; } = string.Empty;
        public string PageLimit { get; set; } = "1";
        public int WindowWidth { get; set; } = 0;
        public int WindowHeight { get; set; } = 0;

        #endregion

        public string IncludedDomains { get; set; }

        public string IncludeFilter { get; set; }

        public string ExcludeFilter { get; set; }

        public string UserAgent { get; set; }

        public bool IncludeMsofficeDocs { get; set; }

        public bool IncludeOtherDocs { get; set; }

        public bool RetestAllPages { get; set; }

        // Below properties are useful for Monitor and doesn't required to display for Scan definition.
        [JsonIgnore]
        public int AlertMode { get; set; }

        [JsonIgnore]
        public int AlertDelay { get; set; }

        [JsonIgnore]
        public string AlertSendTo { get; set; }

        [JsonIgnore]
        public string AlertSubject { get; set; }
                
        [JsonIgnore]
        public List<StartPage> StartPages { get; set; }

        public DateTimeOffset? DateCreated { get; set; }
        public DateTimeOffset? DateModified { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string LocalClientId { get; set; }
        public Boolean CanEdit { get; set; }

        public override bool Equals(Object obj)
        {
            var isScan = obj is Scan;
            if (!isScan) return false;

            var compareScan = (Scan)obj;

            List<string> originalCheckpointGroups = CheckpointGroupIds.Cast<string>().ToList();
            List<string> updatedCheckpointGroups = compareScan.CheckpointGroupIds.Cast<string>().ToList();

            var chkPointGroupDifferences = ComparisonServices.CompareLists<string>(originalCheckpointGroups, updatedCheckpointGroups);

            List<string> originalIncludeDomainList = IncludedDomains.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Cast<string>().ToList();
            List<string> updatedIncludeDomainList = compareScan.IncludedDomains.Split(new[] { "\r\n", "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries).Cast<string>().ToList();

            var includedDomainDifferences = ComparisonServices.CompareLists<string>(originalIncludeDomainList, updatedIncludeDomainList);            

            var startPagesSame = StartPages.SequenceEqual(compareScan.StartPages);

            return ScanId == compareScan.ScanId &&
                   IsMonitor == compareScan.IsMonitor &&
                   BaseUrl == compareScan.BaseUrl &&
                   DisplayName == compareScan.DisplayName &&
                   chkPointGroupDifferences == 0 &&
                   includedDomainDifferences == 0 &&
                   startPagesSame == true &&
                   IncludeFilter == compareScan.IncludeFilter &&
                   ExcludeFilter == compareScan.ExcludeFilter &&
                   UserAgent == compareScan.UserAgent &&
                   IncludeMsofficeDocs == compareScan.IncludeMsofficeDocs &&
                   IncludeOtherDocs == compareScan.IncludeOtherDocs &&
                   RetestAllPages == compareScan.RetestAllPages &&
                   AlertMode == compareScan.AlertMode &&
                   AlertDelay == compareScan.AlertDelay &&
                   AlertSendTo == compareScan.AlertSendTo &&
                   AlertSubject == compareScan.AlertSubject &&
                   LocalClientId == compareScan.LocalClientId &&
                   Path == compareScan.Path &&
                   Levels == compareScan.Levels &&
                   Script == compareScan.Script &&
                   UserName == compareScan.UserName &&
                   Password == compareScan.Password &&
                   Domain == compareScan.Domain &&
                   PageLimit == compareScan.PageLimit &&
                   WindowWidth == compareScan.WindowWidth &&
                   WindowHeight == compareScan.WindowHeight;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
