using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff.Requests
{
    public class GetCheckpointGroupsByRequest
    {
        public int? ScanGroupId { get; set; }
        public int? ScanId { get; set; }
        public string CheckpointGroupId { get; set; }
    }
}
