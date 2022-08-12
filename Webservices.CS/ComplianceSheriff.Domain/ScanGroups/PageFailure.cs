using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.ScanGroups
{
    public class PageFailure
    {
        public string PageUrl { get; set; }
        public int CurrentCheckpointFailures { get; set; }
        public int OneRunBackCheckpointFailures { get; set; }
        public int TwoRunsBackCheckpointFailures { get; set; }
        public int Priority1Failures { get; set; }
        public int Priority2Failures { get; set; }
        public int Priority3Failures { get; set; }
    }
}
