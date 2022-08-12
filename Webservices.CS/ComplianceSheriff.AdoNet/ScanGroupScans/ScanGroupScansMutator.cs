using ComplianceSheriff.ScanGroupScans;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.ScanGroupScans
{
    public class ScanGroupScansMutator : IScanGroupScansMutator
    {
        public static readonly CommandBuilder AddScanGroupScanCommand = new CommandBuilder(@"
                INSERT INTO dbo.ScanGroupScans (ScanGroupId, ScanId)
                VALUES(@ScanGroupId, @ScanId)
            ",
               new Dictionary<string, Action<DbParameter>>
               {
                { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
               }
           );

        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public ScanGroupScansMutator(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this._unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task AddScanGroupScan(int scanGroupId, int scanId)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await AddScanGroupScanCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                        { "@ScanGroupId", scanGroupId },
                        { "@ScanId", scanId }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }
    }
}
