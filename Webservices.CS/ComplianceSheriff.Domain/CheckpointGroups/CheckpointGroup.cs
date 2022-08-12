using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace ComplianceSheriff.CheckpointGroups
{
    public class CheckpointGroup
    {
        public string CheckpointGroupId { get; set; }
        public string ShortDescription = String.Empty;
        public string LongDescription = String.Empty;
        public StringCollection Checkpoints = new StringCollection();
        public StringCollection Subgroups = new StringCollection();
    }
}
