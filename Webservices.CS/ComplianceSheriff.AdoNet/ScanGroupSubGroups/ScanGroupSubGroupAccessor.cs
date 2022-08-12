using ComplianceSheriff.ScanGroupSubGroups;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.ScanGroupSubGroups
{
    public class ScanGroupSubGroupAccessor : IScanGroupSubGroupAccessor
    {
        private readonly IConnectionManager connection;
        private readonly ILogger<ScanGroupSubGroupAccessor> _logger;

        #region "SQL Queries"

        static readonly string getScangroupSubGroupsByScanGroupId = @"SELECT * FROM ScanGroupSubgroups 
                                                                          WHERE ScanGroupId = @ScanGroupId";
        #endregion

        public ScanGroupSubGroupAccessor(IConnectionManager connection, ILogger<ScanGroupSubGroupAccessor> logger)
        {
            this.connection = connection;
            this._logger = logger;
        }

        public async Task<List<ScanGroupSubGroup>> GetScanGroupSubGroupsByScanGroupId(int scanGroupId, CancellationToken cancellationToken)
        {
            try
            {
                CommandBuilder commandBuilder = new CommandBuilder(getScangroupSubGroupsByScanGroupId,
                     new Dictionary<string, Action<DbParameter>>
                           {
                             { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 }
                           },
                           System.Data.CommandType.Text
                       );

                var scanGroupScans = new List<ScanGroupSubGroup>();

                using (var command = await commandBuilder.BuildFrom(connection,
                       new Dictionary<string, object>
                       {
                            { "@ScanGroupId", scanGroupId },
                       }, cancellationToken))
                {
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            scanGroupScans.Add(new ScanGroupSubGroup
                            {
                                ScanGroupId = Convert.ToInt32(reader["ScanGroupId"]),
                                SubGroupId = Convert.ToInt32(reader["SubGroupId"]),
                            });
                        }

                        return scanGroupScans;
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
