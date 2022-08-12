using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Checkpoints
{
    public class Issue
    {
        public int? Result { get; set; }
        public string ResultText { get; set; }

        public int? PageCount { get; set; }
    }
}
