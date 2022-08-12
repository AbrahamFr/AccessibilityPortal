using ComplianceSheriff.Configuration;
using ComplianceSheriff.UserGroups;
using ComplianceSheriff.Users;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.UserGroups
{
    public class UserGroupMutator : IUserGroupMutator
    {
        #region "Command Builder objects"
            public static readonly CommandBuilder AddUserGroupCmd = new CommandBuilder(@"
                                INSERT INTO UserGroups(
                                    Name,
                                    CanRunLocal,
                                    CanEditGlobalLists
                                )
                                VALUES (@Name, @CanRunLocal, @CanEditGlobalLists)
                                SET @ID = SCOPE_IDENTITY()",
                        new Dictionary<string, Action<DbParameter>>
                        {
                                    { "@Name", p => p.DbType = System.Data.DbType.AnsiString },
                                    { "@CanRunLocal", p => p.DbType = System.Data.DbType.Boolean },
                                    { "@CanEditGlobalLists", p => p.DbType = System.Data.DbType.Boolean },
                                    { "@ID", p => { p.DbType = System.Data.DbType.Int32; p.Direction = System.Data.ParameterDirection.Output; } }
                        }
             );

             public static readonly CommandBuilder DeleteUserGroupByIdCmd = new CommandBuilder(@"
                                DELETE dbo.UserGroups WHERE UserGroupId = @UserGroupId",
                    new Dictionary<string, Action<DbParameter>>
                    {
                                    { "@UserGroupId", p => p.DbType = System.Data.DbType.AnsiString },
                    }
            );

            public static readonly CommandBuilder UpdateScanGroupIdCmd = new CommandBuilder(@"
                                    UPDATE dbo.UserGroups 
                                    SET ScanGroupId = @ScanGroupId
                                    WHERE UserGroupId = @UserGroupId",
                    new Dictionary<string, Action<DbParameter>>
                    {
                                    { "@ScanGroupId", p => p.DbType = System.Data.DbType.AnsiString },
                                    { "@UserGroupId", p => p.DbType = System.Data.DbType.AnsiString },
                    }
            );

            public static readonly CommandBuilder UpdateUserGroupNameCmd = new CommandBuilder(@"
                                        UPDATE dbo.UserGroups 
                                        SET Name = @UserGroupName
                                        WHERE UserGroupId = @UserGroupId",
                    new Dictionary<string, Action<DbParameter>>
                    {
                                     { "@UserGroupName", p => p.DbType = System.Data.DbType.AnsiString },
                                     { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                    }
            );



        #endregion

        private readonly IConnectionManager _connection;
        private readonly ILogger<UserGroupMutator> _logger;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public UserGroupMutator(IConnectionManager connection, IUnitOfWorkFactory unitOfWorkFactory, ILogger<UserGroupMutator> logger)
        {
            this._logger = logger;
            this._connection = connection;
            this._unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task<int> AddUserGroup(string userGroupName)
        {
            int userGroupId = 0;

            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await AddUserGroupCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@Name", userGroupName },
                        { "@CanRunLocal", Convert.ToBoolean(0) },
                        { "@CanEditGlobalLists",  Convert.ToBoolean(0) }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                    userGroupId = Convert.ToInt32(command.Parameters["@ID"].Value.ToString());
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }

            return userGroupId;
        }
        
        public async Task UpdateUserGroupName(int userGroupId, string userGroupName)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await UpdateUserGroupNameCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserGroupId", userGroupId },
                        { "@UserGroupName", userGroupName }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }


        public async Task DeleteUserGroupById(int userGroupId)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await DeleteUserGroupByIdCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserGroupId", userGroupId }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }

        public async Task UpdateScanGroupId(int userGroupId, int scanGroupId)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await UpdateScanGroupIdCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserGroupId", userGroupId },
                        { "@ScanGroupId", scanGroupId }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }
    }
}
