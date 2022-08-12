using ComplianceSheriff.Runs;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.Runs
{
    public class RunAccessor : IRunAccessor
    {
        private readonly IConnectionManager _connection;

        #region "SQL Queries"

            #region "GetRunStatusByScanId"
            static readonly string sqlGetRunStatusByScanId = @"
                                SELECT Status FROM Runs
                                Where RunId = (Select MAX(RunId) FROM Runs 
                                               WHERE ScanId = @ScanId)";
            #endregion

            #region "GetRunByRunId"
            static readonly string sqlGetRunByRunId = @"
                                    SELECT RunId, ScanId, Started, Finished, 
                                           Status, TaskId, AbortReason, ScanGroupId, 
                                           Health, ScanGroupRunId, ScheduledScan 
                                    FROM Runs
                                    Where RunId = @RunId";
            #endregion

            #region "GetLastCompletedRunIdForScanId"
            static readonly string sqlGetLastCompletedRunIdForScanId = @"
                                        SELECT ISNULL(Max(RunId),0) As LastRunId
                                        FROM Runs
                                        Where Status = 2 AND ScanId = @ScanId";
            #endregion

        #endregion

        public RunAccessor(IConnectionManager connection)
        {
            _connection = connection;
        }

        public async Task<int> GetRunStatusByScanId(int scanId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetRunStatusByScanId,
               new Dictionary<string, Action<DbParameter>>
                   {
                        { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                   },
                    System.Data.CommandType.Text
               );

            int status = 0;

            using (var command = await commandBuilder.BuildFrom(_connection,
                             new Dictionary<string, object>
                             {
                                { "@ScanId", scanId }
                             }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        status = Convert.ToInt32(reader["Status"].ToString());  
                    }
                }
            }

            return status;
        }

        public async Task<Run> GetRunByRunId(int runId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetRunByRunId,
               new Dictionary<string, Action<DbParameter>>
                   {
                        { "@RunId", p => p.DbType = System.Data.DbType.Int32 }
                   },
                    System.Data.CommandType.Text
               );

            Run run = null; 

            using (var command = await commandBuilder.BuildFrom(_connection,
                             new Dictionary<string, object>
                             {
                                { "@RunId", runId }
                             }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        run = new Run
                        {
                            RunId = Convert.ToInt32(reader["RunId"].ToString()),
                            ScanId = Convert.ToInt32(reader["ScanId"].ToString()),
                            ScanGroupId = int.TryParse(reader["ScanGroupId"].ToString(), out int outValue) ? (int?)outValue : null,
                            ScanGroupRunId = int.TryParse(reader["ScanGroupRunId"].ToString(), out int scanGroupRunOutValue) ? (int?)scanGroupRunOutValue : null,
                            Started = DateTime.TryParse(reader["Started"].ToString(), out DateTime startedOutValue) ? (DateTime?)startedOutValue : null,
                            Finished = !String.IsNullOrEmpty(reader["Finished"].ToString()) ? Convert.ToDateTime(reader["Finished"].ToString()) : (DateTime?)System.Data.SqlTypes.SqlDateTime.Null,
                            AbortReason = reader["AbortReason"].ToString(),
                            Health = int.TryParse(reader["Health"].ToString(), out int healthOutValue) ? (int?)healthOutValue : null,
                            TaskId = reader["TaskId"].ToString(),
                            Status = Convert.ToInt32(reader["Status"].ToString()),
                            ScheduledScan = int.TryParse(reader["ScheduledScan"].ToString(), out int scheduledScanOutValue) ? (int?)scheduledScanOutValue : null
                        };
                        
                    }
                }
            }

            return run;
        }

        public async Task<int> GetLastCompletedRunIdForScanId(int scanId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetLastCompletedRunIdForScanId,
               new Dictionary<string, Action<DbParameter>>
                   {
                        { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                   },
                    System.Data.CommandType.Text
               );
            int lastRunId = 0;
            using (var command = await commandBuilder.BuildFrom(_connection,
                             new Dictionary<string, object>
                             {
                                { "@ScanId", scanId }
                             }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        lastRunId = Convert.ToInt32(reader["LastRunId"].ToString());                           
                    }
                }
            }
            return lastRunId;
        }
    }
}
