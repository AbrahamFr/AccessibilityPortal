using ComplianceSheriff.UserInfos;
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

namespace ComplianceSheriff.AdoNet.UserInfos
{
    public class UserInfoMutator : IUserInfoMutator
    {
        private readonly IConnectionManager _connection;
        private readonly ILogger<UserInfoMutator> _logger;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        #region "Command Builders"

        public readonly CommandBuilder AddUserInfoCmd = new CommandBuilder(@"
                    INSERT INTO [dbo].[UserInfos](
                        [UserInfoId],
                        [DashboardMode],
                        [DashboardViews],
                        [TimeZone],
                        [MaxShortLength],
                        [MaxLongLength],
                        [MaxUrlLength],
                        [UseScriptEditor],
                        [AutoUpdate],
                        [LastScanAccessed],
                        [CurrentScanGroupFilter],
                        [AutoUpdateInterval],
                        [EmailAddress],
                        [StyleSheet],
                        [DateModified],
                        [DateCreated]
                    )
                    VALUES (@UserInfoId, @DashboardMode, @DashboardViews, @TimeZone, @MaxShortLength, 
                            @MaxLongLength, @MaxUrlLength, @UseScriptEditor, @AutoUpdate, 
                            @LastScanAccessed, @CurrentScanGroupFilter, @AutoUpdateInterval,
                            @EmailAddress, @StyleSheet, @DateModified, @DateCreated)",
                     new Dictionary<string, Action<DbParameter>>
                     {
                        { "@UserInfoId", p => p.DbType = System.Data.DbType.AnsiString },
                        { "@DashboardMode", p => p.DbType = System.Data.DbType.Int32 },
                        { "@DashboardViews", p => p.DbType = System.Data.DbType.AnsiString },
                        { "@TimeZone", p => p.DbType = System.Data.DbType.String },
                        { "@MaxShortLength", p => p.DbType = System.Data.DbType.Int32 },
                        { "@MaxLongLength", p => p.DbType = System.Data.DbType.Int32 },
                        { "@MaxUrlLength", p => p.DbType = System.Data.DbType.Int32 },
                        { "@UseScriptEditor", p => p.DbType = System.Data.DbType.Boolean },
                        { "@AutoUpdate", p => p.DbType = System.Data.DbType.Boolean },
                        { "@LastScanAccessed", p => p.DbType = System.Data.DbType.Int32 },
                        { "@CurrentScanGroupFilter", p => p.DbType = System.Data.DbType.Int32 },
                        { "@AutoUpdateInterval", p => p.DbType = System.Data.DbType.Int32 },
                        { "@EmailAddress", p => p.DbType = System.Data.DbType.AnsiString },
                        { "@StyleSheet", p => p.DbType = System.Data.DbType.AnsiString },
                        { "@DateModified", p => p.DbType = System.Data.DbType.DateTime },
                        { "@DateCreated", p => p.DbType = System.Data.DbType.DateTime }
                     }
                 );


        public readonly CommandBuilder UpdateUserInfoCmd = new CommandBuilder(@"
                    Update [dbo].[UserInfos]
                         SET [TimeZone] = @TimeZone,
                             [EmailAddress] = @EmailAddress,
                             [DateModified] = @DateModified
                    WHERE UserInfoId = @UserInfoId",

                      new Dictionary<string, Action<DbParameter>>
                      {
                        { "@UserInfoId", p => p.DbType = System.Data.DbType.AnsiString },                        
                        { "@TimeZone", p => p.DbType = System.Data.DbType.String },
                        { "@EmailAddress", p => p.DbType = System.Data.DbType.AnsiString },
                        { "@DateModified", p => p.DbType = System.Data.DbType.DateTime },
                      }
                  );
        #endregion


        public UserInfoMutator(IConnectionManager connection, IUnitOfWorkFactory unitOfWorkFactory, ILogger<UserInfoMutator> logger)
        {
            _connection = connection;
            _logger = logger;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task AddUserInfo(UserInfo userInfo, CancellationToken cancellationToken)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using var command = await AddUserInfoCmd.BuildFrom(_connection, new Dictionary<string, object>
                    {
                        { "@UserInfoId", userInfo.UserInfoId },
                        { "@DashboardMode", userInfo.DashboardMode },
                        { "@DashboardViews", userInfo.DashboardViews },
                        { "@TimeZone", userInfo.TimeZone },
                        { "@MaxShortLength", userInfo.MaxShortLength },
                        { "@MaxLongLength", userInfo.MaxLongLength },
                        { "@MaxUrlLength", userInfo.MaxUrlLength },
                        { "@UseScriptEditor", userInfo.UseScriptEditor },
                        { "@AutoUpdate", userInfo.AutoUpdate },
                        { "@LastScanAccessed", userInfo.LastScanAccessed ?? (object)DBNull.Value },
                        { "@CurrentScanGroupFilter", userInfo.CurrentScanGroupFilter ?? (object)DBNull.Value },
                        { "@AutoUpdateInterval", userInfo.AutoUpdateInterval },
                        { "@EmailAddress", userInfo.EmailAddress },
                        { "@StyleSheet", userInfo.StyleSheet },
                        { "@DateModified", userInfo.DateModified },
                        { "@DateCreated", userInfo.DateCreated }
                    }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }

        public void UpdateUserInfo(UserInfo userInfo, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
        {
            unitOfWork.DeferSql(async (connection, cancellationToken) =>
            {
                using var command = await UpdateUserInfoCmd.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserInfoId", userInfo.UserInfoId },
                    { "@TimeZone", userInfo.TimeZone },
                    { "@EmailAddress", userInfo.EmailAddress },
                    { "@DateModified", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") },
                }, cancellationToken);

                await command.ExecuteNonQueryAsync(cancellationToken);
            });
        }


        public async Task RemoveUserInfo(string userInfoId, CancellationToken cancellationToken)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    CommandBuilder DeleteUserProfileCmd = new CommandBuilder($@"
                    DELETE dbo.UserInfos
                    WHERE UserInfoId = @UserInfoId",
                            new Dictionary<string, Action<DbParameter>>
                            {
                            { "@UserInfoId", p => p.DbType = System.Data.DbType.String }
                            }
                    );

                    using var command = await DeleteUserProfileCmd.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserInfoId", userInfoId }
                }, cancellationToken);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                });

                await unitOfWork.CommitAsync(CancellationToken.None);
            }
        }
    }
}
