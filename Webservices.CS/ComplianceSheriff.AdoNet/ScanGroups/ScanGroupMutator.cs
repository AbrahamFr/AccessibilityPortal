using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.ScanGroups
{
    public class ScanGroupMutator : IScanGroupMutator
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public ScanGroupMutator(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this._unitOfWorkFactory = unitOfWorkFactory;
        }

        public ScanGroupMutator()
        {

        }

        public static readonly CommandBuilder AddScanGroupCommand = new CommandBuilder(@"
                DECLARE @NewScanGroupId int
                Select @NewScanGroupId = ISNULL(MAX(ScanGroupId) + 1,2000) FROM ScanGroups Where ScanGroupId < 9999

                SET IDENTITY_INSERT ScanGroups ON
                  INSERT INTO dbo.ScanGroups (ScanGroupId, DisplayName, DateCreated, DateModified)
                  VALUES(@NewScanGroupId, @DisplayName, GETDATE(), GETDATE())
                SET IDENTITY_INSERT ScanGroups OFF

                SET @ID = @NewScanGroupId
               ",
                new Dictionary<string, Action<DbParameter>>
                {
                    { "@DisplayName", p => p.DbType = System.Data.DbType.AnsiString },
                    { "@ID", p => { p.DbType = System.Data.DbType.Int32; p.Direction = System.Data.ParameterDirection.Output; } },
                }
            );

        public static readonly CommandBuilder UpdateScanGroupNameCommand = new CommandBuilder(@"
                UPDATE dbo.ScanGroups
                SET DisplayName=@Name,
                    DateModified=GETDATE()
                WHERE ScanGroupId=@ScanGroupId
               ",
                new Dictionary<string, Action<DbParameter>>
                {
                    { "@Name", p => p.DbType = System.Data.DbType.AnsiString },
                    { "@ScanGroupId", p => p.DbType = System.Data.DbType.Int32 },
                }
            );

        public void UpdateScanGroupName(int scanGroupId, string name, IUnitOfWork unitOfWork)
        {
            unitOfWork.DeferSql(async (connection, cancellationToken) =>
            {
                using (var command = await UpdateScanGroupNameCommand.BuildFrom(connection, new Dictionary<string, object>
                {
                    { "@Name", name },
                    { "@ScanGroupId", scanGroupId }
                }, cancellationToken))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            });
        }

        public async Task<int> AddScanGroup(string scanGroupName)
        {
            int newScanGroupId = 0;

            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await AddScanGroupCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                        { "@DisplayName", scanGroupName }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                        newScanGroupId = Convert.ToInt32(command.Parameters["@ID"].Value.ToString());
                    }
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }

            return newScanGroupId;
        }





    }
}
