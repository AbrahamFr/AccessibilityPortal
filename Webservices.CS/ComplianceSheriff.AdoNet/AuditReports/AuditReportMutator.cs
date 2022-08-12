using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using ComplianceSheriff.AuditReports;
using System.Data.Common;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.AuditReports
{
    public class AuditReportMutator : IAuditReportMutator
    {

        public static readonly CommandBuilder UpdateAuditReportCommand = new CommandBuilder(@"
                UPDATE dbo.AuditReports
                SET AuditTypeId = @AuditTypeId,
                    ReportName = @ReportName,                                        
                    ReportDescription = @ReportDescription,
                    LastModifiedDate = @LastModifiedDate
                WHERE AuditReportId = @AuditReportId",
                 new Dictionary<string, Action<DbParameter>>
                 {
                    { "@AuditReportId", p => p.DbType = System.Data.DbType.Int32 },
                    { "@AuditTypeId", p => p.DbType = System.Data.DbType.Int32 },
                    { "@ReportName", p => p.DbType = System.Data.DbType.AnsiString },                    
                    { "@ReportDescription", p => p.DbType = System.Data.DbType.AnsiString },
                    { "@LastModifiedDate", p => p.DbType = System.Data.DbType.DateTime },
                 }
             );
        public static readonly CommandBuilder InsertAuditReportCommand = new CommandBuilder(@"
                INSERT INTO dbo.AuditReports(
                    AuditTypeId,
                    ReportName,
                    FileType,
                    FileLocation,                    
                    FileSize,
                    FileUploadDate,                         
                    ReportDescription,
                    FileStatusId,
                    LastModifiedDate
                )
                VALUES (@AuditTypeId, @ReportName, @FileType, @FileLocation, @FileSize, @FileUploadDate, @ReportDescription, @FileStatusId, @LastModifiedDate)
                ",
                 new Dictionary<string, Action<DbParameter>>
                 {
                    { "@AuditTypeId", p => p.DbType = System.Data.DbType.Int32 },
                    { "@ReportName", p => p.DbType = System.Data.DbType.AnsiString },                                                                                
                    { "@FileType", p => p.DbType = System.Data.DbType.String },                    
                    { "@FileLocation", p => p.DbType = System.Data.DbType.String },
                    { "@FileSize", p => p.DbType = System.Data.DbType.Int64 },
                    { "@FileUploadDate", p => p.DbType = System.Data.DbType.DateTime },
                    { "@ReportDescription", p => p.DbType = System.Data.DbType.AnsiString },
                    { "@FileStatusId", p => p.DbType = System.Data.DbType.Int32 },
                    { "@LastModifiedDate", p => p.DbType = System.Data.DbType.DateTime }
                 }
             );

        static readonly CommandBuilder DeleteAuditReportCommand = new CommandBuilder(@"
                   
                    DELETE p
                        FROM UserGroupPermissions p
                        INNER JOIN UserGroups g
	                      ON p.UserGroupId = g.UserGroupId
                      WHERE Type = 'AuditReport'
	                    AND TypeId = CONVERT(NVARCHAR, @AuditReportId)
	                    AND g.UserGroupId = @UserGroupId
                    
                    DELETE dbo.AuditReports WHERE AuditReportId = @AuditReportId
                ",
                 new Dictionary<string, Action<DbParameter>>
                 {
                    { "@AuditReportId", p => p.DbType = System.Data.DbType.Int32 },
                    { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                 }
             );

        static readonly CommandBuilder UpdateAuditReportFileStatusCommand = new CommandBuilder(@"
                   UPDATE dbo.AuditReports 
                    SET FileStatusId = @FileStatusId,
                        LastModifiedDate = @LastModifiedDate
                    WHERE AuditReportId = @AuditReportId
                ",
                 new Dictionary<string, Action<DbParameter>>
                 {
                    { "@AuditReportId", p => p.DbType = System.Data.DbType.Int32 },
                    { "@FileStatusId", p => p.DbType = System.Data.DbType.Int16 },
                    { "@LastModifiedDate", p => p.DbType = System.Data.DbType.DateTime },
                 }
             );
        public void UpdateAuditReport(AuditReport auditReport, IUnitOfWork unitOfWork)
        {
            unitOfWork.DeferSql(async (connection, cancellationToken) =>
            {
                using (var command = await UpdateAuditReportCommand.BuildFrom(connection, new Dictionary<string, object>
                {
                    { "@AuditReportId",auditReport.AuditReportId },
                    { "@AuditTypeId", auditReport.AuditTypeId },
                    { "@ReportName", auditReport.ReportName },                    
                    { "@ReportDescription", auditReport.ReportDescription },
                    { "@LastModifiedDate", DateTime.UtcNow }
                }, cancellationToken))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            });
        }
        public void AddAuditReport(AuditReport auditReport, IUnitOfWork unitOfWork)
        {
            unitOfWork.DeferSql(async (connection, cancellationToken) =>
            {
                using (var command = await InsertAuditReportCommand.BuildFrom(connection, new Dictionary<string, object>
                {
                    { "@AuditTypeId", auditReport.AuditTypeId },
                    { "@ReportName", auditReport.ReportName },
                    { "@FileType", auditReport.FileType },
                    { "@FileLocation", auditReport.FileLocation },
                    { "@FileSize", auditReport.FileSize },
                    { "@FileUploadDate", auditReport.FileUploadDate },
                    { "@ReportDescription", auditReport.ReportDescription },
                    { "@FileStatusId", auditReport.FileStatusId },
                    { "@LastModifiedDate", auditReport.LastModifiedDate }
                }, cancellationToken))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            });
        }

        public void DeleteAuditReport(int auditReportId, int userGroupId, IUnitOfWork unitOfWork)
        {
            unitOfWork.DeferSql(async (connection, cancellationToken) =>
            {
                using (var command = await DeleteAuditReportCommand.BuildFrom(connection, new Dictionary<string, object>
                {
                    { "@AuditReportId", auditReportId },
                    { "@UserGroupId", userGroupId }
                }, cancellationToken))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            });
        }

        public void UpdateAuditReportFileStatus(int auditReportId, int fileStatusId, IUnitOfWork unitOfWork)
        {
            unitOfWork.DeferSql(async (connection, cancellationToken) =>
            {
                using (var command = await UpdateAuditReportFileStatusCommand.BuildFrom(connection, new Dictionary<string, object>
                {
                    { "@AuditReportId", auditReportId },
                    { "@FileStatusId", fileStatusId },
                    { "@LastModifiedDate", DateTime.UtcNow }
                }, cancellationToken))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            });
        }
    }
}
