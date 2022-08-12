using ComplianceSheriff.Configuration;
using ComplianceSheriff.Users;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.Users
{
    public class UserAccessor : IUserAccessor
    {
        private readonly IConnectionManager _connection;
        private readonly ConfigurationOptions _configOptions;
        private readonly ILogger<UserAccessor> _logger;

        public UserAccessor(IConnectionManager connection, IOptions<ConfigurationOptions> configOptions, ILogger<UserAccessor> logger)
        {
            this._connection = connection;
            this._configOptions = configOptions.Value;
            this._logger = logger;
        }


        public async Task<User> GetUserByUserId(int userId, CancellationToken cancellationToken)
        {
            try
            {
                var getUserByUserIdsql = $@"SELECT u.UserId, 
                                                     u.Name,
                                                     u.FirstName,
                                                     u.LastName,
                                                     u.Password, 
                                                     um.OrganizationId, 
                                                     EmailAddress,
                                                     TimeZone,
                                                     u.TempPassword,
                                                     u.VerificationToken
                                              FROM Users u
                                              INNER JOIN {this._configOptions.ClusterName}_main.dbo.UserMapper um
                                                ON u.UserId = um.OrgUserId
                                              INNER JOIN dbo.UserInfos ui
                                                ON u.Name = ui.UserInfoId
                                              WHERE u.UserId = @UserId";

                var getUserCommand = new CommandBuilder(getUserByUserIdsql,
                     new Dictionary<string, Action<DbParameter>>
                     {
                        { "@UserId", p => p.DbType = System.Data.DbType.Int32 }
                     },
                     System.Data.CommandType.Text
                 );

                using (var command = await getUserCommand.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserId", userId }
                }, cancellationToken))
                {
                    _logger.LogInformation("Begin Retrieving User by UserId");

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var user = new User
                            {
                                UserId = Convert.ToInt32(reader["UserId"].ToString()),
                                Name = reader["Name"].ToString(),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Password = reader["Password"].ToString(),
                                OrganizationId = reader["OrganizationId"].ToString(),
                                EmailAddress = reader["EmailAddress"].ToString(),
                                TimeZone = reader["TimeZone"].ToString(),
                                TempPassword = reader["TempPassword"].ToString(),
                                VerificationToken = reader["VerificationToken"].ToString(),
                                Identity = new GenericIdentity(reader["Name"].ToString())
                            };

                            return user;
                        }
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                throw ex;
            }
            finally
            {
                _logger.LogInformation("End Retrieving User by UserId");
            }
        }


        public async Task<User> GetUserByUserName(string userName, CancellationToken cancellationToken)
        {
            try
            {
                var getUserByUserNamesql = $@"SELECT u.UserId, 
                                                     u.Name,
                                                     u.FirstName,
                                                     u.LastName,
                                                     u.Password, 
                                                     um.OrganizationId, 
                                                     EmailAddress,
                                                     TimeZone
                                              FROM Users u
                                              INNER JOIN {this._configOptions.ClusterName}_main.dbo.UserMapper um
                                                ON u.UserId = um.OrgUserId
                                              INNER JOIN dbo.UserInfos ui
                                                ON u.Name = ui.UserInfoId
                                              WHERE u.Name = @UserName";

                var getUserCommand = new CommandBuilder(getUserByUserNamesql,
                     new Dictionary<string, Action<DbParameter>>
                     {
                        { "@UserName", p => p.DbType = System.Data.DbType.String }
                     },
                     System.Data.CommandType.Text
                 );

                using (var command = await getUserCommand.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserName", userName }
                }, cancellationToken))
                {
                    _logger.LogInformation("Begin Retrieving User by Username");

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var user = new User {
                                UserId = Convert.ToInt32(reader["UserId"].ToString()),
                                Name = reader["Name"].ToString(),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Password = reader["Password"].ToString(),
                                OrganizationId = reader["OrganizationId"].ToString(),
                                EmailAddress = reader["EmailAddress"].ToString(),
                                TimeZone = reader["TimeZone"].ToString()
                            };

                            return user;
                        }
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                throw ex;
            }
            finally
            {
                _logger.LogInformation("End Retrieving User by Username");
            }
        }

        public async Task<User> GetUserRecordByUserName(string userName, CancellationToken cancellationToken)
        {
            try
            {
                var getUserByUserNamesql = $@"SELECT u.UserId, 
                                                     u.UserGroupId,
                                                     u.Name,
                                                     u.Password,
                                                     u.FirstName,
                                                     u.LastName,
                                                     u.TempPassword,
                                                     u.VerificationToken
                                              FROM Users u
                                              WHERE u.Name = @UserName";

                var getUserCommand = new CommandBuilder(getUserByUserNamesql,
                     new Dictionary<string, Action<DbParameter>>
                     {
                        { "@UserName", p => p.DbType = System.Data.DbType.String }
                     },
                     System.Data.CommandType.Text
                 );

                using (var command = await getUserCommand.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@UserName", userName }
                }, cancellationToken))
                {
                    _logger.LogInformation("Begin Retrieving User by Username");

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var user = new User { 
                                
                                UserId = Convert.ToInt32(reader["UserId"].ToString()),
                                UserGroupId = Convert.ToInt32(reader["UserGroupId"].ToString()),
                                Name = reader["Name"].ToString(),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Password = reader["Password"].ToString(),
                                TempPassword = reader["TempPassword"].ToString(),
                                VerificationToken = reader["VerificationToken"].ToString(),
                                Identity = new GenericIdentity(reader["Name"].ToString())
                            };

                            return user;
                        }
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
                throw ex;
            }
            finally
            {
                _logger.LogInformation("End Retrieving User by Username");
            }
        }
    }
}
