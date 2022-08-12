using ComplianceSheriff.ApiRoles;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.ApiRoles
{
    public class ApiRolesAccessor : IApiRoleAccessor
    {
        private readonly IConnectionManager _connection;
        private readonly ILogger<ApiRolesAccessor> _logger;

        #region "SQL queries"

        private static readonly string sqlGetApiRoles = @"
              SELECT * FROM [dbo].[udfGetApiRolesForUserGroupName] (@UserGroupName)";

        #endregion

        public ApiRolesAccessor(IConnectionManager connection, ILogger<ApiRolesAccessor> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<IEnumerable<ApiRole>> GetApiRolesByUserGroupName(string userGroupName, CancellationToken cancellationToken)
        {
            CommandBuilder GetUserCommand = new CommandBuilder(sqlGetApiRoles,
               new Dictionary<string, Action<DbParameter>>
               {
                    { "@UserGroupName", p => p.DbType = System.Data.DbType.String }
               },
               System.Data.CommandType.Text
            );

            var apiRoleList = new List<ApiRole>();

            using (var command = await GetUserCommand.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserGroupName", userGroupName }
                }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        apiRoleList.Add(new ApiRole {
                                    JwtRoleId = Convert.ToInt32(reader["JwtRoleId"].ToString()),
                                    JwtRoleName = reader["JwtRoleName"].ToString()    
                         });
                    }
                }
            }

            return apiRoleList;
        }
    }
}
