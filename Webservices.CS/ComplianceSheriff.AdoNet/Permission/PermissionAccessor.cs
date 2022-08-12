using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using ComplianceSheriff.Permission;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;

namespace ComplianceSheriff.AdoNet.Permission
{
    public class PermissionAccessor : IPermissionAccessor
    {
        #region "SQL Queries"
       
        static readonly string sqlGetUserGroupIdForPermission = @"
                  Select ug.UserGroupId 
                  from UserGroups ug 
                  INNER JOIN Users u on ug.UserGroupId = u.UserGroupId
                  Where u.Name = @UserName";

        static readonly string sqlCheckScanPermissions = @"

                      Select s.ScanId 
					  FROM Scans s 
					  INNER JOIN [dbo].[udfScansByPermission](@UserGroupId, @PermissionType) sbp
					   ON sbp.ScanId = s.ScanId
					  INNER JOIN UserGroupPermissions ugp
					   ON (ugp.TypeId = '*' OR CAST(s.ScanId AS nvarchar(32)) = ugp.TypeId)
					  AND ugp.Type = @PermissionType
					  Where s.ScanId = @ScanId
						AND ugp.Action = @Action
						AND ugp.UserGroupId = @UserGroupId";

        static readonly string sqlCheckPermissionTypeAndActionForUserGroup = @"
                      Select ugp.PermissionId
                      FROM UserGroupPermissions ugp
                      Where ugp.Type = @PermissionType AND
                            ugp.Action = @Action AND
                            ugp.UserGroupId = @UserGroupId AND
                            (ugp.TypeId = '*' OR ugp.TypeId = CAST(@ObjectId AS nvarchar(32)))";

        #endregion


        private readonly IConnectionManager connection;
        private readonly ILogger<PermissionAccessor> _logger;

        public PermissionAccessor(IConnectionManager connection, ILogger<PermissionAccessor> logger)
        {
            this.connection = connection;
            _logger = logger;
        }

        public async Task<Int32> GetUserGroupIdForPermission(string userName, CancellationToken cancellationToken)
        {
            var userGroupId = 0;
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetUserGroupIdForPermission,
                       new Dictionary<string, Action<DbParameter>>
                       {
                           { "@UserName", p => p.DbType = System.Data.DbType.String }                          
                       },
                       System.Data.CommandType.Text
                   );

            

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@UserName", userName}
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        userGroupId = Convert.ToInt32(reader["UserGroupid"].ToString());                        
                    }
                }
            }

            return userGroupId;
        }

        public async Task<bool> CheckScanRunPermission(int scanId, int userGroupId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlCheckScanPermissions,
                        new Dictionary<string, Action<DbParameter>>
                        {
                           { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
                           { "@UserGroupId", p => p.DbType = System.Data.DbType.String },
                           { "@PermissionType", p => p.DbType = System.Data.DbType.String },
                           { "@Action", p => p.DbType = System.Data.DbType.String }
                        },
                        System.Data.CommandType.Text
                    );

            bool hasPermission = false;

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@ScanId", scanId },
                            { "@UserGroupId", userGroupId },
                            { "@PermissionType", "Scan" },
                            { "@Action" , "edit" }
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    hasPermission = reader.HasRows;
                }
            }

            return hasPermission;
        }

        public async Task<bool> CheckPermissionTypeAndActionForUserGroup(int objectId, int userGroupId, string permissionType, string permissionAction, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlCheckPermissionTypeAndActionForUserGroup,
                        new Dictionary<string, Action<DbParameter>>
                        {
                           { "@ObjectId", p => p.DbType = System.Data.DbType.Int32 },
                           { "@UserGroupId", p => p.DbType = System.Data.DbType.String },
                           { "@PermissionType", p => p.DbType = System.Data.DbType.String },
                           { "@Action", p => p.DbType = System.Data.DbType.String }
                        },
                        System.Data.CommandType.Text
                    );

            bool hasPermission = false;

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@ObjectId", objectId },
                            { "@UserGroupId", userGroupId },
                            { "@PermissionType", permissionType },
                            { "@Action" , permissionAction }
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    hasPermission = reader.HasRows;
                }
            }

            return hasPermission;
        }
    }
}
