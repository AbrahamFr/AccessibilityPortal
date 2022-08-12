using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.LogMessages
{
    public interface ILogMessagesAccessor
    {
        Task<List<LogMessagesItem>> GetLogMessagesRecord(string loggerRunId, CancellationToken cancellationToken);
    }
}