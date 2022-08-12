using ComplianceSheriff.APILogger;
using ComplianceSheriff.Configuration;
using ComplianceSheriff.Logging;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.APILogger
{
    public class APILoggerMutator : IAPILoggerMutator
    {
        #region SQL Scripts
            private readonly string addApiLoggerRecord = @"INSERT INTO [dbo].[APIAuditLogs]
                                                           ([UserName]
                                                           ,[APIEndpoint]
                                                           ,[Method]
                                                           ,[Organization]
                                                           ,[Parameters]
                                                           ,[UserAgent]
                                                           ,[RequestTime]
                                                           ,[JsonWebToken])
                                                     VALUES
                                                           (@UserName,
                                                            @APIEndpoint,
                                                            @Method,
                                                            @Organization,
                                                            @Parameters,
                                                            @UserAgent,
                                                            @RequestTime,
                                                            @JsonWebToken)";

        #endregion

        private readonly ILogger<APILoggerMutator> _logger;
        private readonly ConfigurationOptions _configOptions;

        public APILoggerMutator(IUnitOfWorkFactory unitOfWorkFactory, IOptions<ConfigurationOptions> options, ILogger<APILoggerMutator> logger)
        {
            this._logger = logger;
            _configOptions = options.Value;
        }
    
        public async void AddAPILogger(APILoggerRequest apiLoggerRequest)
        {
            try
            {
                using (var connection = new SqlConnection(_configOptions.APILoggerConnection))
                {
                    if (connection.State == System.Data.ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    CommandBuilder InsertAPILoggerCommand = new CommandBuilder(addApiLoggerRecord,
                        new Dictionary<string, Action<DbParameter>>
                        {
                            { "@UserName", p => p.DbType = System.Data.DbType.String },
                            { "@APIEndpoint", p => p.DbType = System.Data.DbType.String },
                            { "@Method", p => p.DbType = System.Data.DbType.String },
                            { "@Organization", p => p.DbType = System.Data.DbType.String },
                            { "@Parameters", p => p.DbType = System.Data.DbType.String },
                            { "@RequestTime", p => p.DbType = System.Data.DbType.String },
                            { "@JsonWebToken", p => p.DbType = System.Data.DbType.String },
                            { "@UserAgent", p => p.DbType = System.Data.DbType.String }
                        });

                    using (var command = InsertAPILoggerCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                        { "@UserName", apiLoggerRequest.UserName },
                        { "@APIEndpoint", apiLoggerRequest.APIEndpoint },
                        { "@Method", apiLoggerRequest.Method },
                        { "@Organization", apiLoggerRequest.Organization },
                        { "@Parameters", apiLoggerRequest.Parameters },
                        { "@RequestTime", apiLoggerRequest.RequestTime },
                        { "@JsonWebToken", apiLoggerRequest.JsonWebToken },
                        { "@UserAgent", apiLoggerRequest.UserAgent }
                    }))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace);
            }
        }
    }
}
