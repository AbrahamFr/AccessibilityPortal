using ComplianceSheriff.Permission;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.Permission
{
    public class PermissionMutator : IPermissionMutator
    {
        private readonly ILogger<PermissionMutator> _logger;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public PermissionMutator(IUnitOfWorkFactory unitOfWorkFactory, ILogger<PermissionMutator> logger)
        {
            this._logger = logger;
            this._unitOfWorkFactory = unitOfWorkFactory;
        }
        static readonly CommandBuilder InsertUserPermissionCommand = new CommandBuilder(@"

               IF NOT EXISTS(SELECT * FROM UserGroupPermissions
								WHERE UserGroupId = @UserGroupId
									AND Type = @Type
									AND Action = @Action
									AND TypeId = '*'
								UNION 
								SELECT * FROM UserGroupPermissions
								WHERE UserGroupId = @UserGroupId
									AND Type = @Type
									AND Action = @Action
									AND TypeId = @TypeId
								)
                BEGIN
                    INSERT INTO dbo.UserGroupPermissions(
                            [UserGroupId]
                            ,[Type]
                            ,[TypeId]
                            ,[Action]
                    )
                    VALUES (@UserGroupId, @Type, @TypeId, @Action)
                END",
                  new Dictionary<string, Action<DbParameter>>
                  {
                    { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                    { "@Type", p => p.DbType = System.Data.DbType.String },
                    { "@TypeId", p => p.DbType = System.Data.DbType.String },
                    { "@Action", p => p.DbType = System.Data.DbType.String },                    
                  }
              );

        static readonly CommandBuilder DeleteUserPermissionCommand = new CommandBuilder(@"
                    DELETE UserGroupPermissions
			        WHERE UserGroupId = @UserGroupId",
                  new Dictionary<string, Action<DbParameter>>
                  {
                    { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                  }
              );

        public void AddUserGroupPermissionRecord(UserGroupPermission userGroupPermission, IUnitOfWork unitOfWork)
        {
            try
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await InsertUserPermissionCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                    { "@UserGroupId", userGroupPermission.UserGroupId },
                    { "@Type", userGroupPermission.Type },
                    { "@TypeId", userGroupPermission.TypeId },
                    { "@Action", userGroupPermission.Action },
                    }, cancellationToken))
                    {                        
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("AddUserGroupPermissionRecord - ErrorMsg:" + ex.Message + Environment.NewLine + "Stack Trace:" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task DeleteUserGroupPermissionsByUserGroupId(int userGroupId)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await DeleteUserPermissionCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                        { "@UserGroupId", userGroupId },
                        }, cancellationToken);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }
    }
}
