using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.ScanGroupScans;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.ScanGroupScans
{
    public class ScanGroupScansAccessor : IScanGroupScansAccessor
    {
        #region "SQL Queries"

        static readonly string getScanGroupScanByScanGroupIdAndScanId = @"SELECT * FROM ScanGroupScans 
                                                                          WHERE ScanGroupId = @ScanGroupId
                                                                            AND ScanId = @ScanId";
        #endregion

        private readonly IConnectionManager connection;
        private readonly ILogger<ScanGroupScansAccessor> _logger;

        public ScanGroupScansAccessor(IConnectionManager connection, ILogger<ScanGroupScansAccessor> logger)
        {
            this.connection = connection;
            this._logger = logger;
        }

        public async Task<ScanGroupScan> GetScanGroupByScanGroupIdAndScanId(int scanGroupId, int scanId, CancellationToken cancellationToken)
        {
            try
            {
                CommandBuilder commandBuilder = new CommandBuilder(getScanGroupScanByScanGroupIdAndScanId,
                     new Dictionary<string, Action<DbParameter>>
                           {
                             { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                             { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                           },
                           System.Data.CommandType.Text
                       );

                using (var command = await commandBuilder.BuildFrom(connection,
                       new Dictionary<string, object>
                       {
                            { "@ScanGroupId", scanGroupId },
                            { "@ScanId", scanId }
                       }, cancellationToken))
                {
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        ScanGroupScan scanGroupScan = null;

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            scanGroupScan = new ScanGroupScan
                            {
                                ScanGroupId = Convert.ToInt32(reader["ScanGroupId"]),
                                ScanId = Convert.ToInt32(reader["ScanId"]),
                            };
                        }

                        return scanGroupScan;
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
