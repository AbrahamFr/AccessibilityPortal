using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Checkpoints
{
    public class CheckpointFailure
    {
        public string CheckpointId { get; set; }
        public string Description { get; set; }
        public int CurrentFailures { get; set; }
        public int Priority1Failures { get; set; }
        public int OneRunBackFailures { get; set; }
        public int TwoRunsBackFailures { get; set; }
        public  int PagesImpacted { get; set; }
    }
}
