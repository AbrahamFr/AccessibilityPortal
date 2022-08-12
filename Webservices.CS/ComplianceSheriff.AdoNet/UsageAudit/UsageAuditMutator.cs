using ComplianceSheriff.UsageAudit;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace ComplianceSheriff.AdoNet.UsageAudit
{
    public class UsageAuditMutator : IUsageAuditMutator
    {
        static readonly CommandBuilder InsertUserAuditCommand = new CommandBuilder(@"
                INSERT INTO dbo.UserAudit(
                      [UserName],
                      [Type],
                      [ActionType],
                      [Activity],
                      [Changes],
                      [SearchObject],
                      [RecordDate],
                      [IsXML],
                      [AdditionalInformation]
                )
                VALUES (@UserName, @Type, @ActionType, @Activity, @Changes, @SearchObject, @RecordDate, @IsXML, @AdditionalInformation)
                ",
                  new Dictionary<string, Action<DbParameter>>
                  {
                    { "@UserName", p => p.DbType = System.Data.DbType.String },
                    { "@Type", p => p.DbType = System.Data.DbType.Int32 },
                    { "@ActionType", p => p.DbType = System.Data.DbType.String },
                    { "@Activity", p => p.DbType = System.Data.DbType.String },
                    { "@Changes", p => p.DbType = System.Data.DbType.String },
                    { "@SearchObject", p => p.DbType = System.Data.DbType.String },
                    { "@RecordDate", p => p.DbType = System.Data.DbType.DateTime },
                    { "@IsXML", p => p.DbType = System.Data.DbType.Boolean },                    
                    { "@AdditionalInformation", p => p.DbType = System.Data.DbType.AnsiString },
                  }
              );

        public void AddUsageAuditRecord(UserAudit userAudit, IUnitOfWork unitOfWork)
        {
            unitOfWork.DeferSql(async (connection, cancellationToken) =>
            {
                using (var command = await InsertUserAuditCommand.BuildFrom(connection, new Dictionary<string, object>
                {
                    { "@UserName", userAudit.UserName },
                    { "@Type", userAudit.UserAuditTypeId },
                    { "@ActionType", userAudit.ActionTypeId },
                    { "@Activity", userAudit.Activity },
                    { "@Changes", userAudit.Changes },
                    { "@SearchObject", userAudit.SearchObject },
                    { "@RecordDate", userAudit.RecordDate },
                    { "@IsXML", userAudit.IsXml },
                    { "@AdditionalInformation", userAudit.AdditionalInformation },
                }, cancellationToken))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            });
        }
    }
}
