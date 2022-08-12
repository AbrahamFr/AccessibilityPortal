using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ComplianceSheriff.LogMessages;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;

namespace ComplianceSheriff.AdoNet.LogMessages
{
    public class LogMessagesAccessor : ILogMessagesAccessor
    {
        #region "RunLogMessagesList Query"
        public static string sqlRunLogMessagesListQry = @"
                SELECT  Logger
                        ,Timestamp
                        ,Severity
                        ,Message
                        ,StackTrace
                FROM LogMessages 
                Where Logger = @loggerRunId
                Order By MessageId";
        #endregion

        private readonly IConnectionManager _connection;
        private readonly ILogger<LogMessagesAccessor> _logger;

        public LogMessagesAccessor(IConnectionManager connection,
                                   ILogger<LogMessagesAccessor> logger)
        {            
            _connection = connection;            
            _logger = logger;
        }

        public async Task<List<LogMessagesItem>> GetLogMessagesRecord( string loggerRunId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlRunLogMessagesListQry,
                        new Dictionary<string, Action<DbParameter>>
                        {
                            { "@loggerRunId", p => p.DbType = System.Data.DbType.String }                           
                        },
                        System.Data.CommandType.Text
                    );

            var runLogMessagesList = new List<LogMessagesItem>();

            using (var command = await commandBuilder.BuildFrom(_connection,
                  new Dictionary<string, object>
                  {
                      { "@loggerRunId", loggerRunId }
                  }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        runLogMessagesList.Add(new LogMessagesItem
                        {
                            Logger = reader["Logger"].ToString(),
                            Timestamp = Convert.ToDateTime(reader["Timestamp"].ToString()),
                            Severity = Convert.ToInt16(reader["Severity"].ToString()),
                            Message = reader["Message"].ToString(),
                            StackTrace = reader["StackTrace"].ToString()
                        });
                    }
                }
            }
            return runLogMessagesList;
        }
    }
}
