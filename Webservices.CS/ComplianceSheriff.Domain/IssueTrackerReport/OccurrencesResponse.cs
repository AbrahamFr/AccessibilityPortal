using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.IssueTrackerReport
{
    public class OccurrencesResponse
    {
        public List<OccurrenceItem> OccurrencesList { get; set; }
        public List<KeyValuePair<string, string>> TitleFilterList { get; set; }
        public List<string> UrlFilterList { get; set; }
        public List<KeyValuePair<string, string>> KeyAttributeFilterList { get; set; }
        public List<KeyValuePair<string, string>> ElementFilterList { get; set; }
        public List<KeyValuePair<string, string>> ContainerIdFilterList { get; set; }
        public int TotalOccurrences { get; set; }
        public int TotalFilteredRecords { get; set; }

        public OccurrencesResponse()
        {
            OccurrencesList = new List<OccurrenceItem>();
            TitleFilterList = new List<KeyValuePair<string, string>>();
            UrlFilterList = new List<string>();
            KeyAttributeFilterList = new List<KeyValuePair<string, string>>();
            ElementFilterList = new List<KeyValuePair<string, string>>();
            ContainerIdFilterList = new List<KeyValuePair<string,string>>();
        }

    }
}
