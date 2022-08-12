using ComplianceSheriff.Authentication;
using System;
using System.Data.Common;
using DeKreyConsulting.AdoTestability;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq;
using Microsoft.Extensions.Logging;
using ComplianceSheriff.Configuration;
using Microsoft.Extensions.Options;
using ComplianceSheriff.Users;

namespace ComplianceSheriff.AdoNet.Authentication
{
    public class AuthAccessor : IAuthAccessor
    {
        private readonly IConnectionManager connection;
        private readonly ConfigurationOptions configurationOptions;
        private readonly ILogger<AuthAccessor> _logger;

        public AuthAccessor(IConnectionManager connection, IOptions<ConfigurationOptions> options, ILogger<AuthAccessor> logger)
        {
            this.connection = connection;
            this.configurationOptions = options.Value;
            _logger = logger;
        }

        public async Task<User> AuthenticateUser(string userName, string passWord, CancellationToken cancellationToken)
        {
            try
            {
                var authenticateUserCommand = new CommandBuilder("Select Name, Password FROM Users where Name = @UserName And Password = @Password",
                             new Dictionary<string, Action<DbParameter>>
                             {
                                { "@UserName", p => p.DbType = System.Data.DbType.String },
                                { "@Password", p => p.DbType = System.Data.DbType.String },
                             },
                             System.Data.CommandType.Text
                         );

                using (var command = await authenticateUserCommand.BuildFrom(connection, new Dictionary<string, object>
                {
                    { "@UserName", userName },
                    { "@Password", EncryptPassword(passWord) },
                }, cancellationToken))
                {
                    _logger.LogInformation("Begin Authenticating User");

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {                    
                        if(await reader.ReadAsync(cancellationToken))
                        {
                            return new User(reader["Name"].ToString(), reader["Password"].ToString());
                        }
                    }
                   
                    return null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                _logger.LogInformation("End Authenticating User");
            }
         }

        //public async Task<UserMapper> AuthenticateUserProfile(string userName, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var authenticateUserCommand = new CommandBuilder($"Select * FROM {configurationOptions.ClusterName}_main.dbo.UserProfile where EmailAddress = @UserName",
        //                     new Dictionary<string, Action<DbParameter>>
        //                     {
        //                        { "@UserName", p => p.DbType = System.Data.DbType.String },
        //                     },
        //                     System.Data.CommandType.Text
        //                 );

        //        using var command = await authenticateUserCommand.BuildFrom(connection, new Dictionary<string, object>
        //        {
        //            { "@UserName", userName },
        //        }, cancellationToken);

        //        _logger.LogInformation("Begin Authenticating Compliance Investigate User");

        //        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        //        {
        //            if (await reader.ReadAsync(cancellationToken))
        //            {
        //                return new ComplianceSheriff.UserProfiles.UserMapper
        //                {
        //                    EmailAddress = reader["EmailAddress"].ToString(),
        //                    FirstName = reader["FirstName"].ToString(),
        //                    LastName = reader["LastName"].ToString(),
        //                    OrganizationId = reader["OrganizationId"].ToString(),
        //                    OrgUserId = !string.IsNullOrWhiteSpace(reader["OrgUserId"].ToString()) ? Convert.ToInt32(reader["OrgUserId"].ToString()) : (int?)null
        //                };
        //            }
        //        }

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        _logger.LogInformation("End Authenticating Compliance Investigate User");
        //    }
        //}

        internal string EncryptPassword(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                return String.Join("", sha1.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("x2").ToUpperInvariant()).ToArray());
            }
        }
    }
}
