using ComplianceSheriff.UserGroups;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.UserGroups
{
    public class UserGroupAccessor : IUserGroupAccessor
    {
        private readonly IConnectionManager _connection;
        private readonly ILogger<UserGroupAccessor> _logger;

        #region "SQL Queries"

        public static readonly string getUserGroupByUserName = @"                
                SELECT Top 1 ug.UserGroupId, ug.Name, CanRunLocal, CanEditGlobalLists, ScanGroupId
                FROM UserGroups ug
                INNER JOIN Users u
                 ON ug.UserGroupId = u.UserGroupId
                WHERE u.Name = @UserName";

        public static readonly string getUserGroupByGroupName = @"
                SELECT * FROM UserGroups
                WHERE [Name] = @UserGroupName";

        public static readonly string getUserGroupById = @"
                SELECT * FROM UserGroups
                WHERE UserGroupId = @UserGroupId";

        #endregion

        public UserGroupAccessor(IConnectionManager connection, ILogger<UserGroupAccessor> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<UserGroup> GetUserGroupByUserName(string userName, CancellationToken cancellationToken)
        {
            CommandBuilder GetUserCommand = new CommandBuilder(getUserGroupByUserName,
               new Dictionary<string, Action<DbParameter>>
               {
                    { "@UserName", p => p.DbType = System.Data.DbType.String }
               },
               System.Data.CommandType.Text
            );

            using (var command = await GetUserCommand.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserName", userName }
                }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        return new UserGroup()
                        {
                            UserGroupId = Convert.ToInt32(reader["UserGroupId"].ToString()),
                            Name = reader["Name"].ToString(),
                            ScanGroupId = String.IsNullOrWhiteSpace(reader["ScanGroupId"].ToString()) ? (int?)null : Convert.ToInt32(reader["ScanGroupId"].ToString())
                        };
                    }
                }
            }

            return null;
        }

        public async Task<UserGroup> GetUserGroupByUserGroupName(string userGroupName, CancellationToken cancellationToken)
        {
            CommandBuilder GetUserCommand = new CommandBuilder(getUserGroupByGroupName,
               new Dictionary<string, Action<DbParameter>>
               {
                    { "@UserGroupName", p => p.DbType = System.Data.DbType.String }
               },
               System.Data.CommandType.Text
            );

            using (var command = await GetUserCommand.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserGroupName", userGroupName }
                }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        return new UserGroup()
                        {
                            UserGroupId = Convert.ToInt32(reader["UserGroupId"].ToString()),
                            Name = reader["Name"].ToString()
                        };
                    }
                }
            }

            return null;
        }

        public async Task<UserGroup> GetUserGroupById(int userGroupId, CancellationToken cancellationToken)
        {
            CommandBuilder GetUserGroupByIdCommand = new CommandBuilder(getUserGroupById,
               new Dictionary<string, Action<DbParameter>>
               {
                    { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 }
               },
               System.Data.CommandType.Text
            );

            using (var command = await GetUserGroupByIdCommand.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserGroupId", userGroupId }
                }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        return new UserGroup()
                        {
                            UserGroupId = Convert.ToInt32(reader["UserGroupId"].ToString()),
                            Name = reader["Name"].ToString()
                        };
                    }
                }
            }

            return null;
        }
    }
}
