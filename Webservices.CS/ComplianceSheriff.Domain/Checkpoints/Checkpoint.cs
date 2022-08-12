using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Checkpoints
{
    public class Checkpoint
    {
        public string CheckpointId { get; set; }

        public string LongDescription { get; set; }
        public string ShortDescription { get; set; }

        public string Number { get; set; }

        public string Description { get; set; }

        public int Volume { get; set; }
    }
}
