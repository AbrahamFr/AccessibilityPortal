using ComplianceSheriff.Configuration;
using ComplianceSheriff.Passwords;
using ComplianceSheriff.UserInfos;
using ComplianceSheriff.Users;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.Users
{
    public class UserMutator : IUserMutator
    {
        private readonly IConnectionManager _connection;
        private readonly ConfigurationOptions _configOptions;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPasswordService _passwordService;

        #region "Command Builders"

            public readonly CommandBuilder AddUserCmd = new CommandBuilder(@"
                        INSERT INTO Users(
                            UserGroupId,                        
                            Name,                        
                            Password,
                            FirstName,
                            LastName,
                            ForceChangePassword,
                            LastModifiedDate
                        )
                        VALUES (@UserGroupId, @Name, @Password, @FirstName, @LastName, @ForceChangePassword, @LastModifiedDate)
                        SET @ID = SCOPE_IDENTITY()",
                         new Dictionary<string, Action<DbParameter>>
                         {
                            { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                            { "@Name", p => p.DbType = System.Data.DbType.AnsiString },
                            { "@Password", p => p.DbType = System.Data.DbType.String },
                            { "@FirstName", p => p.DbType = System.Data.DbType.String },
                            { "@LastName", p => p.DbType = System.Data.DbType.String },
                            { "@ForceChangePassword", p => p.DbType = System.Data.DbType.Int32 },
                            { "@LastModifiedDate", p => p.DbType = System.Data.DbType.AnsiString },
                            { "@ID", p => { p.DbType = System.Data.DbType.Int32; p.Direction = System.Data.ParameterDirection.Output; } }
                         }
                     );

        public readonly CommandBuilder UpdateUserNameCmd = new CommandBuilder(@"
                        Update Users
                            SET Name = @UserName,
                                LastModifiedDate = @LastModifiedDate
                        WHERE UserId = @UserId",
                      new Dictionary<string, Action<DbParameter>>
                      {
                            { "@UserId", p => p.DbType = System.Data.DbType.Int32 },
                            { "@UserName", p => p.DbType = System.Data.DbType.String },
                            { "@LastModifiedDate", p => p.DbType = System.Data.DbType.AnsiString }
                      }
                  );

        public readonly CommandBuilder UpdateUserCmd = new CommandBuilder(@"
                        Update Users
                            SET FirstName = @FirstName,
                                LastName = @LastName,
                                LastModifiedDate = @LastModifiedDate
                        WHERE UserId = @UserId",
                         new Dictionary<string, Action<DbParameter>>
                         {
                            { "@UserId", p => p.DbType = System.Data.DbType.Int32 },
                            { "@FirstName", p => p.DbType = System.Data.DbType.String },
                            { "@LastName", p => p.DbType = System.Data.DbType.String },
                            { "@LastModifiedDate", p => p.DbType = System.Data.DbType.AnsiString }
                         }
                     );

            public readonly CommandBuilder SetTempPasswordCmd = new CommandBuilder(@"
                            UPDATE Users
                                SET TempPassword = @TempPassword,
                                    VerificationToken = @VerificationToken,
                                    LastModifiedDate = @LastModifiedDate
                            WHERE UserId = @UserId",
                          new Dictionary<string, Action<DbParameter>>
                          {
                                { "@UserId", p => p.DbType = System.Data.DbType.Int32 },
                                { "@TempPassword", p => p.DbType = System.Data.DbType.String },
                                { "@VerificationToken", p => p.DbType = System.Data.DbType.String },
                                { "@LastModifiedDate", p => p.DbType = System.Data.DbType.AnsiString }
                          }
                      );

            public readonly CommandBuilder ResetUserPasswordCmd = new CommandBuilder(@"
                            Update Users
                                SET Password = @Password,
                                    TempPassword = NULL,
	                                VerificationToken = NULL
                            WHERE UserId = @UserId",
                          new Dictionary<string, Action<DbParameter>>
                          {
                                { "@UserId", p => p.DbType = System.Data.DbType.Int32 },
                                { "@Password", p => p.DbType = System.Data.DbType.String },
                                { "@LastModifiedDate", p => p.DbType = System.Data.DbType.AnsiString }
                          }
                      );

            public readonly CommandBuilder UpdateUserPasswordCmd = new CommandBuilder(@"
                                Update Users
                                  SET Password = @Password
                                WHERE UserId = @UserId",
                          new Dictionary<string, Action<DbParameter>>
                          {
                                    { "@UserId", p => p.DbType = System.Data.DbType.Int32 },
                                    { "@Password", p => p.DbType = System.Data.DbType.String },
                                    { "@LastModifiedDate", p => p.DbType = System.Data.DbType.AnsiString }
                          }
                      );


        #endregion

        public UserMutator(IConnectionManager connection,
                           IPasswordService passwordService,
                           IUnitOfWorkFactory unitOfWorkFactory,
                           IOptions<ConfigurationOptions> options, ILogger<UserMutator> logger)
        {
            _connection = connection;
            _configOptions = options.Value;
            _unitOfWorkFactory = unitOfWorkFactory;
            _passwordService = passwordService;

        }

        public async Task<int> AddUser(User user)
        {
            int userId = 0;

            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await AddUserCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserGroupId", user.UserGroupId },
                        { "@Name", user.Name },
                        { "@Password", user.Password },
                        { "@FirstName", user.FirstName },
                        { "@LastName", user.LastName },
                        { "@ForceChangePassword", 0 },
                        { "@LastModifiedDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                    userId = Convert.ToInt32(command.Parameters["@ID"].Value.ToString());
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }

            return userId;
        }

        public async Task UpdateUserName(string userName, int userId)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await UpdateUserNameCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserId", userId },
                        { "@UserName", userName },
                        { "@LastModifiedDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }

        public void UpdateUser(User user, IUnitOfWork unitOfWork)
        {
            unitOfWork.DeferSql(async (connection, cancellationToken) =>
            {
                using var command = await UpdateUserCmd.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserId", user.UserId },
                    { "@FirstName", user.FirstName },
                    { "@LastName", user.LastName },
                    { "@LastModifiedDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") }
                }, cancellationToken);

                await command.ExecuteNonQueryAsync(cancellationToken);
            });
        }

        public async Task RemoveUser(User user)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    CommandBuilder DeleteUserCmd = new CommandBuilder($@"
                        DELETE Users
                        WHERE UserId = @UserId",
                            new Dictionary<string, Action<DbParameter>>
                            {
                                { "@UserId", p => p.DbType = System.Data.DbType.Int32 }
                            }
                    );

                    using var command = await DeleteUserCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserId", user.Name }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }

        public void AddUserMapper(User user, IUnitOfWork unitOfWork)
        {
            unitOfWork.DeferSql(async (connection, cancellationToken) =>
            {
                CommandBuilder AddUserMapperCmd = new CommandBuilder($@"
                    INSERT INTO {_configOptions.ClusterName}_main.dbo.UserMapper(
                        OrganizationId,
                        OrgUserId,
                        CreateDate,
                        LastModifiedDate
                    )
                    VALUES (@OrganizationId, @OrgUserId, @CreateDate, @LastModifiedDate)
                    ",
                        new Dictionary<string, Action<DbParameter>>
                        {
                            { "@OrganizationId", p => p.DbType = System.Data.DbType.String },
                            { "@OrgUserId", p => p.DbType = System.Data.DbType.Int32 },
                            { "@CreateDate", p => p.DbType = System.Data.DbType.DateTime },
                            { "@LastModifiedDate", p => p.DbType = System.Data.DbType.DateTime }
                        }
                );

                using var command = await AddUserMapperCmd.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@OrganizationId", user.OrganizationId },
                    { "@OrgUserId", user.OrgUserId },
                    { "@CreateDate",  DateTime.UtcNow },
                    { "@LastModifiedDate", DateTime.UtcNow }
                }, cancellationToken);

                await command.ExecuteNonQueryAsync(cancellationToken);
            });
        }

        public async Task RemoveUserMapper(User user)
        {

            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    CommandBuilder DeleteUserProfileCmd = new CommandBuilder($@"
                    DELETE {_configOptions.ClusterName}_main.dbo.UserMapper
                    WHERE OrgUserId = @UserId",
                            new Dictionary<string, Action<DbParameter>>
                            {
                            { "@UserId", p => p.DbType = System.Data.DbType.Int32 }
                            }
                    );

                    using var command = await DeleteUserProfileCmd.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserId", user.UserId }
                }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }

        public async Task SetTempPasswordAndVerificationToken(User user, string tempPassword, HashResult hashResult)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await SetTempPasswordCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserId", user.UserId },
                        { "@TempPassword", tempPassword },
                        { "@VerificationToken",  hashResult.Hash },
                        { "@LastModifiedDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }

        public async Task UnsetTempPassword(User user)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await SetTempPasswordCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserId", user.UserId },
                        { "@TempPassword", String.Empty },
                        { "@LastModifiedDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }

        public async Task ResetUserPassword(int userId, string password)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await ResetUserPasswordCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserId", userId },
                        { "@Password", password },
                        { "@LastModifiedDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }

        public async Task UpdateUserPassword(int userId, string password)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await UpdateUserPasswordCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserId", userId },
                        { "@Password", password },
                        { "@LastModifiedDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }
    }
}
