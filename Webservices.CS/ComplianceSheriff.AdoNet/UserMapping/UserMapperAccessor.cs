using ComplianceSheriff.Configuration;
using ComplianceSheriff.Exceptions;
using ComplianceSheriff.UserMapping;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.UserMapping
{
    public class UserMapperAccessor : IUserMapperAccessor
    {
        private readonly IConnectionManager _connection;
        private readonly ConfigurationOptions _configOptions;
        private readonly ILogger<UserMapperAccessor> _logger;

        public UserMapperAccessor(IConnectionManager connection, IOptions<ConfigurationOptions> configOptions, ILogger<UserMapperAccessor> logger)
        {
            this._connection = connection;
            this._configOptions = configOptions.Value;
            this._logger = logger;
        }

        public async Task<UserMapper> GetUserMapperByOrgUserId(int orgUserId, CancellationToken cancellationToken)
        {
            try
            {
                var getUserCommand = new CommandBuilder($"Select * FROM {this._configOptions.ClusterName}_main.dbo.UserMapper where OrgUserId = @OrgUserId",
                     new Dictionary<string, Action<DbParameter>>
                     {
                        { "@OrgUserId", p => p.DbType = System.Data.DbType.Int32 }
                     },
                     System.Data.CommandType.Text
                 );

                using (var command = await getUserCommand.BuildFrom(_connection, new Dictionary<string, object>
                {
                    { "@OrgUserId", orgUserId }
                }, cancellationToken))
                {
                    _logger.LogInformation("Begin Retrieving UserMapper by OrgUserId");

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            return new UserMapper
                            {
                                UserMapperId = Convert.ToInt32(reader["UserMapperId"].ToString()),
                                OrganizationId = reader["OrganizationId"].ToString(),
                                OrgUserId = !string.IsNullOrWhiteSpace(reader["OrgUserId"].ToString()) ? Convert.ToInt32(reader["OrganizationId"].ToString()) : (int?)null
                            };
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
                _logger.LogInformation("End Retrieving UserMapper by OrgUserId");
            }
        }
    }
}
