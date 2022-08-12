using ComplianceSheriff.Checkpoints;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ComplianceSheriff.AdoNet.Checkpoints
{
    public class IssuesAccessor : IIssuesAccessor
    {
        private readonly IConnectionManager connection;
        private readonly ILogger<IssuesAccessor> _logger;

        //static readonly string top10IssuesCommandText = "GetTopIssuesByScanGroup";

        //public static readonly CommandBuilder GetTop10IssuesCommand = new CommandBuilder(top10IssuesCommandText,
        //             new Dictionary<string, Action<DbParameter>>
        //             {
        //                { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
        //                { "@StartDate", p => p.DbType = System.Data.DbType.DateTime },
        //                { "@FinishDate", p => p.DbType = System.Data.DbType.DateTime },
        //             },
        //             System.Data.CommandType.StoredProcedure
        //         );

        public IssuesAccessor(IConnectionManager connection, ILogger<IssuesAccessor> logger)
        {
            this.connection = connection;
            _logger = logger;
        }

        public async Task<IEnumerable<Issue>> GetTop10Issues(int? scanGroupId, DateRange range, CancellationToken cancellationToken)
        {
            try
            {
                CommandBuilder commandBuilder;
                string commandText;

                if (scanGroupId == null || scanGroupId <= 0)
                {
                    commandText = "GetTopIssuesByScanGroup_default";
                }
                else
                {
                    commandText = "GetTopIssuesByScanGroup";
                }

                commandBuilder = new CommandBuilder(commandText,
                      new Dictionary<string, Action<DbParameter>>
                      {
                        { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@StartDate", p => p.DbType = System.Data.DbType.DateTime },
                        { "@FinishDate", p => p.DbType = System.Data.DbType.DateTime },
                      },
                      System.Data.CommandType.StoredProcedure
                  );

                using (var command = await commandBuilder.BuildFrom(connection,
                      new Dictionary<string, object>
                      {
                    { "@ScanGroupId", scanGroupId },
                    { "@StartDate", range.StartDate?.DateTime },
                    { "@FinishDate", range.EndDate?.DateTime },
                      }, cancellationToken))
                {
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        var top10Issues = new List<Issue>();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            top10Issues.Add(new Issue
                            {
                                Result = Convert.ToInt32(reader["ResultStatus"].ToString()),
                                ResultText = reader["ResultText"].ToString(),
                                PageCount = Convert.ToInt32(reader["Total"].ToString())
                            });
                        };

                        return top10Issues;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
                throw;
            }
        }
    }
}
