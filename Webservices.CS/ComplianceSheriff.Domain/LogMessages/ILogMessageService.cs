using ComplianceSheriff.Work;

namespace ComplianceSheriff.LogMessages
{
    public interface ILogMessageService
    {
        void LogMessage(LogMessagesItem logMessageItem, IUnitOfWorkFactory unitOfOWorkFactory);
    }
}
