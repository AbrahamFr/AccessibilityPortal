using System;
using ComplianceSheriff.Enums;

namespace ComplianceSheriff.Scans
{
    public class ActiveTask
    {
        public string Handle { get; set; }
        public ActiveTaskState State { get; set; }
        public string Body { get; set; }
        public Int64 HostAddress { get; set; }
    }
}
