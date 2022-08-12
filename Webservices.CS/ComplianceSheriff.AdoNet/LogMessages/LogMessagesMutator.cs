using System;
using System.Collections.Generic;
using System.Data.Common;
using ComplianceSheriff.LogMessages;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;

namespace ComplianceSheriff.AdoNet.LogMessages
{
    public class LogMessagesMutator : ILogMessagesMutator
    {        
        #region "Command objects"
        public static readonly CommandBuilder AddLogMessageCommand = new CommandBuilder(@"
                                        INSERT INTO LogMessages
                                            (Logger,
                                             TimeStamp,
                                             Severity,
                                             Message)
                                        VALUES (
                                             @Logger,
                                             @TimeStamp,
                                             @Severity,
                                             @Message",

                       new Dictionary<string, Action<DbParameter>>
                       {
                                { "@Logger", p => p.DbType = System.Data.DbType.String },
                                { "@TimeStamp", p => p.DbType = System.Data.DbType.DateTime },
                                { "@Severity", p => p.DbType = System.Data.DbType.Int16 },
                                { "@Message", p => p.DbType = System.Data.DbType.String },
                       });
        #endregion

        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public LogMessagesMutator(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this._unitOfWorkFactory = unitOfWorkFactory;
        }

        public void AddLogMessagesRecord(LogMessagesItem logMessageItem)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await AddLogMessageCommand.BuildFrom(connection, new Dictionary<string, object>
                {
                    { "@Logger", logMessageItem.Logger },
                    { "@TimeStamp", logMessageItem.Timestamp },
                    { "@Severity", logMessageItem.Severity },
                    { "@Message", logMessageItem.Message }
                }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });
            }
        }
    }
}
