using ComplianceSheriff.UserInfos;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.UserInfos
{
    public class UserInfoAccessor : IUserInfoAccessor
    {
        private readonly IConnectionManager _connection;
        private readonly ILogger<UserInfoAccessor> _logger;

        public UserInfoAccessor(IConnectionManager connection, ILogger<UserInfoAccessor> logger)
        {
            this._connection = connection;
            this._logger = logger;
        }

        public async Task<UserInfo> GetUserInfoByUserInfoId(string userInfoId, CancellationToken cancellationToken)
        {
            var getUserInfoByUserInfoIdsql = @"SELECT *
                                              FROM UserInfos
                                              WHERE UserInfoId = @UserInfoId";

            var getUserInfosCommand = new CommandBuilder(getUserInfoByUserInfoIdsql,
                 new Dictionary<string, Action<DbParameter>>
                 {
                        { "@UserInfoId", p => p.DbType = System.Data.DbType.AnsiString }
                 },
                 System.Data.CommandType.Text
             );

            using (var command = await getUserInfosCommand.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserInfoId", userInfoId }
                }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        var userInfo = new UserInfo
                        {
                            UserInfoId = reader["UserInfoId"].ToString(),
                            DashboardMode = Convert.ToInt32(reader["DashboardMode"].ToString()),
                            DashboardViews = reader["DashboardViews"].ToString(),
                            TimeZone = reader["TimeZone"].ToString(),
                            MaxShortLength = Convert.ToInt32(reader["MaxShortLength"].ToString()),
                            MaxLongLength = Convert.ToInt32(reader["MaxLongLength"].ToString()),
                            MaxUrlLength = Convert.ToInt32(reader["MaxUrlLength"].ToString()),
                            UseScriptEditor = Convert.ToBoolean(reader["UseScriptEditor"].ToString()),
                            AutoUpdate = Convert.ToBoolean(reader["AutoUpdate"].ToString()),
                            LastScanAccessed = Convert.ToInt32(reader["LastScanAccessed"].ToString()),
                            CurrentScanGroupFilter = !String.IsNullOrWhiteSpace(reader["CurrentScanGroupFilter"].ToString()) ? Convert.ToInt32(reader["CurrentScanGroupFilter"].ToString()) : (int?)null,
                            AutoUpdateInterval = Convert.ToInt32(reader["AutoUpdateInterval"].ToString()),
                            EmailAddress = reader["EmailAddress"].ToString(),
                            StyleSheet = reader["StyleSheet"].ToString(),
                            DateModified = Convert.ToDateTime(reader["DateModified"].ToString()),
                            DateCreated = Convert.ToDateTime(reader["DateCreated"].ToString())
                    };

                        return userInfo;
                    }
                }

                return null;
            }
        }
    }
}
