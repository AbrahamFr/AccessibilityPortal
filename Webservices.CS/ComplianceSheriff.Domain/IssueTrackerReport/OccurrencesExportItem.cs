using ComplianceSheriff.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ComplianceSheriff.IssueTrackerReport
{
    [Serializable()]
    public class OccurrencesExportItem
    {
        public string PageTitle { get; set; }
        public string PageUrl { get; set; }
        public string Element { get; set; }
        public string ContainerId { get; set; }
        public string KeyAttributeName { get; set; }
        public string KeyAttributeValue { get; set; }
        public string Issue { get; set; }
        public string ScanGroupName { get; set; }
        public string ScanDisplayName { get; set; }
        public int ScanId { get; set; }
        public string CheckpointGroupName { get; set; }
        public string CheckpointDescription { get; set; }
               
        [XmlIgnore]
        public string CachedPageUrl { get; set; }

        [EpplusIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        [XmlElement("CachedPageUrl")]
        public XmlCDataSection CachedPageUrlCDATA
        {
            get => new System.Xml.XmlDocument().CreateCDataSection(CachedPageUrl);

            set
            {
                CachedPageUrl = value.Value;
            }
        }
    }

}
