using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using ComplianceSheriff.AuditReports;
using DeKreyConsulting.AdoTestability;
using Microsoft.Extensions.Logging;

namespace ComplianceSheriff.AdoNet.AuditReports
{
    public class AuditReportAccessor : IAuditReportAccessor
    {
        #region "SQL Queries"
        public static readonly string sqlGetAuditReportById = @"SELECT * FROM AuditReports r
                                                                    INNER JOIN AuditType t ON r.AuditTypeId = t.AuditTypeId
                                                                    INNER JOIN AuditReportFileStatus fs on fs.FileStatusId = r.FileStatusId
                                                                 WHERE AuditReportId = @AuditReportId";

        
        static readonly string sqlAuditReportByIdWithUserPermission = @"
                Select 
	                ar.AuditReportId,
	                art.AuditTypeName,
                    art.AuditTypeId,
	                ar.ReportName,
	                ar.ReportDescription,
	                ar.FileType,
	                ar.FileLocation,
	                ar.FileSize,
	                ar.FileUploadDate,
                    ar.LastModifiedDate,
                    fs.FileStatusName,
                    fs.FileStatusId,                    
                    arbp.AuditReportPermission
                from AuditReports ar
                join AuditType art on ar.AuditTypeId = art.AuditTypeId
                join AuditReportFileStatus fs on fs.FileStatusId = ar.FileStatusId
		        left outer join dbo.udfGetAuditReportPermission(@UserGroupId, @PermissionType) arbp
		            ON arbp.AuditReportId = ar.AuditReportId
                WHERE ar.AuditReportId = @AuditReportId";


        static readonly string sqlAllAuditReportsList = @"
                Select 
	                ar.AuditReportId,
	                art.AuditTypeName,
                    art.AuditTypeId,
	                ar.ReportName,
	                ar.ReportDescription,
	                ar.FileType,
	                ar.FileLocation,
	                ar.FileSize,
	                ar.FileUploadDate,
                    ar.LastModifiedDate,
                    fs.FileStatusName,
                    fs.FileStatusId,                    
                    arbp.AuditReportPermission
                from AuditReports ar
                join AuditType art on ar.AuditTypeId = art.AuditTypeId
                join AuditReportFileStatus fs on fs.FileStatusId = ar.FileStatusId
		        join dbo.udfGetAuditReportPermission(@UserGroupId, @PermissionType) arbp
		            ON arbp.AuditReportId = ar.AuditReportId
                order by ar.AuditReportId desc
";
        static readonly string sqlGetAuditReportIdByNameText = @"
                Select ar.AuditReportId	                
                from AuditReports ar
                Where ar.ReportName = @ReportName and ar.FileLocation = @FileLocation";

        static readonly string sqlGetAuditReportNameByReportNameText = @"
                Select ar.AuditReportId,
                       ar.ReportName,
                       ar.FileLocation
                from AuditReports ar
                Where ar.ReportName = @ReportName";

        #endregion


        private readonly IConnectionManager connection;
        private readonly ILogger<AuditReportAccessor> _logger;

        public AuditReportAccessor(IConnectionManager connection, ILogger<AuditReportAccessor> logger)
        {
            this.connection = connection;
            _logger = logger;
        }

        public async Task<AuditReport> GetAuditReportWithUserPermission(int userGroupId, int auditReportId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlAuditReportByIdWithUserPermission,
                       new Dictionary<string, Action<DbParameter>>
                       {
                           { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                           { "@PermissionType", p => p.DbType = System.Data.DbType.String },
                           { "@AuditReportId", p => p.DbType = System.Data.DbType.Int32 }
                       },
                       System.Data.CommandType.Text
                   );

            AuditReport auditReport = null;

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@UserGroupId", userGroupId},
                            { "@PermissionType", "AuditReport" },
                            { "@AuditReportId", auditReportId }
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        auditReport = new AuditReport
                        {
                            AuditReportId = Convert.ToInt32(reader["AuditReportId"].ToString()),
                            AuditTypeId = Convert.ToInt32(reader["AuditTypeId"].ToString()),
                            AuditTypeName = reader["AuditTypeName"].ToString(),
                            ReportDescription = reader["ReportDescription"].ToString(),
                            ReportName = reader["ReportName"].ToString(),
                            FileSize = Convert.ToInt64(reader["FileSize"].ToString()),
                            FileType = reader["FileType"].ToString(),
                            FileLocation = reader["FileLocation"].ToString(),
                            FileUploadDate = Convert.ToDateTime(reader["FileUploadDate"].ToString()),
                            FileStatusName = reader["FileStatusName"].ToString(),
                            FileStatusId = Convert.ToInt32(reader["FileStatusId"].ToString()),
                            LastModifiedDate = Convert.ToDateTime(reader["LastModifiedDate"].ToString()),
                            CanEdit = reader["AuditReportPermission"].ToString() == "edit" ? true : false
                        };
                    }
                }
            }

            return auditReport;
        }

        public async Task<List<AuditReport>> GetAllAuditReports(int userGroupId, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlAllAuditReportsList,
                       new Dictionary<string, Action<DbParameter>>
                       {
                           { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                           { "@PermissionType", p => p.DbType = System.Data.DbType.String }
                       },
                       System.Data.CommandType.Text
                   );

            var auditReportList = new List<AuditReport>();

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@UserGroupId", userGroupId},
                            { "@PermissionType", "AuditReport" }
                        },cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var auditReport = new AuditReport
                        {
                            AuditReportId = Convert.ToInt32(reader["AuditReportId"].ToString()),
                            AuditTypeId = Convert.ToInt32(reader["AuditTypeId"].ToString()),
                            AuditTypeName = reader["AuditTypeName"].ToString(),
                            ReportDescription = reader["ReportDescription"].ToString(),
                            ReportName = reader["ReportName"].ToString(),
                            FileSize = Convert.ToInt64(reader["FileSize"].ToString()),
                            FileType = reader["FileType"].ToString(),
                            FileLocation = reader["FileLocation"].ToString(),
                            FileUploadDate = Convert.ToDateTime(reader["FileUploadDate"].ToString()),
                            FileStatusName = reader["FileStatusName"].ToString(),
                            FileStatusId = Convert.ToInt32(reader["FileStatusId"].ToString()),
                            LastModifiedDate = Convert.ToDateTime(reader["LastModifiedDate"].ToString()),
                            CanEdit = reader["AuditReportPermission"].ToString() == "edit"? true : false
                        };

                        auditReportList.Add(auditReport);
                    }
                }
            }

            return auditReportList;
        }
    
        public async Task<AuditReport> GetAuditReportById(int auditReportId, CancellationToken cancellationToken)
        {
            AuditReport auditReport = null;

            CommandBuilder commandBuilder = new CommandBuilder(sqlGetAuditReportById,
                 new Dictionary<string, Action<DbParameter>>
                       {
                            { "@AuditReportId", p => p.DbType = System.Data.DbType.Int32 }
                       },
                       System.Data.CommandType.Text
                   );

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                        { "@AuditReportId", auditReportId },
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        auditReport = new AuditReport
                        {
                            AuditReportId = Convert.ToInt32(reader["AuditReportId"].ToString()),
                            AuditTypeName = reader["AuditTypeName"].ToString(),
                            ReportDescription = reader["ReportDescription"].ToString(),
                            ReportName = reader["ReportName"].ToString(),
                            FileSize = Convert.ToInt64(reader["FileSize"].ToString()),
                            FileType = reader["FileType"].ToString(),
                            FileLocation = reader["FileLocation"].ToString(),
                            FileUploadDate = Convert.ToDateTime(reader["FileUploadDate"].ToString())
                        };
                    }
                }
            }

            return auditReport;
        }

        public async Task<Int32> GetAuditReportId(string reportName, string fileLocation, CancellationToken cancellationToken)
        {
            var auditReportId = 0;

            CommandBuilder commandBuilder = new CommandBuilder(sqlGetAuditReportIdByNameText,
                 new Dictionary<string, Action<DbParameter>>
                       {
                            { "@ReportName", p => p.DbType = System.Data.DbType.String },
                            { "@FileLocation", p => p.DbType = System.Data.DbType.String }
                       },
                       System.Data.CommandType.Text
                   );

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@ReportName", reportName},
                            { "@FileLocation", fileLocation }                       
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        auditReportId = Convert.ToInt32(reader["AuditReportId"].ToString());                           
                    }
                }
            }
            return auditReportId;
        }

        public async Task<List<AuditReport>> GetAuditReportsByReportName(string reportName, CancellationToken cancellationToken)
        {
            CommandBuilder commandBuilder = new CommandBuilder(sqlGetAuditReportNameByReportNameText,
                 new Dictionary<string, Action<DbParameter>>
                       {
                            { "@ReportName", p => p.DbType = System.Data.DbType.String }
                       },
                       System.Data.CommandType.Text
                   );

            var auditReportList = new List<AuditReport>();

            using (var command = await commandBuilder.BuildFrom(connection,
                        new Dictionary<string, object>
                        {
                            { "@ReportName", reportName},
                        }, cancellationToken))
            {
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var auditReport = new AuditReport
                        {
                            AuditReportId = Convert.ToInt32(reader["AuditReportId"].ToString()),
                            ReportName = reader["ReportName"].ToString(),
                            FileLocation = reader["FileLocation"].ToString()
                        };

                        auditReportList.Add(auditReport);
                    }
                }
            }
            return auditReportList;
        }
    }
}
