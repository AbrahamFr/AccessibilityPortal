using System;

namespace ComplianceSheriff.LogMessages
{
    public class LogMessagesItem
    {
        public string Logger { get; set; }
        public DateTime Timestamp { get; set; }
        public Int16 Severity { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
