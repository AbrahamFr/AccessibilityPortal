using ComplianceSheriff.Scans;
using ComplianceSheriff.Work;
using ComplianceSheriff.FileSystem;
using DeKreyConsulting.AdoTestability;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ComplianceSheriff.LogMessages;
using ComplianceSheriff.Configuration;
using Microsoft.Extensions.Options;

namespace ComplianceSheriff.AdoNet.Scans
{
    public class ScanMutator : IScanMutator
    {
        #region CommandBuilder Objects

        #region "AddScanRunCommand"
    
            public static readonly CommandBuilder AddScanRunCommand = new CommandBuilder(@"
                               INSERT INTO Runs(ScanId,
                                                Status,
                                                Started,
                                                ScanGroupId,
                                                ScheduledScan,
                                                ScanGroupRunId)
                                                VALUES(
                                                @ScanId,
                                                @Status,
                                                @Started,
                                                @ScanGroupId,
                                                @ScheduledScan,
                                                @ScanGroupRunId)
                                                SET @ID = SCOPE_IDENTITY()",
                     new Dictionary<string, Action<DbParameter>>
                     {
                            { "@ScanId", p => p.DbType = System.Data.DbType.String },
                            { "@Status", p => p.DbType = System.Data.DbType.Int32 },
                            { "@Started", p => p.DbType = System.Data.DbType.DateTime },
                            { "@ScanGroupId", p => p.DbType = System.Data.DbType.AnsiString },
                            { "@ScheduledScan", p => p.DbType = System.Data.DbType.Boolean },
                            { "@ScanGroupRunId", p => p.DbType = System.Data.DbType.Int32 },
                            { "@ID", p => { p.DbType = System.Data.DbType.Int32; p.Direction = System.Data.ParameterDirection.Output; } },
                         }
                     );

        #endregion

        #region "UpdateScanRunCommand"

        public static readonly CommandBuilder UpdateScanRunCommand = new CommandBuilder(@"
                                    Update Runs 
                                    SET TaskId = @TaskId
                                    WHERE RunId = @RunId",

                          new Dictionary<string, Action<DbParameter>>
                          {
                            { "@TaskId", p => p.DbType = System.Data.DbType.String },
                            { "@RunId", p => p.DbType = System.Data.DbType.Int32 }
                          });

        #endregion

        #region "AbortScanRunCommand"

            public static readonly CommandBuilder AbortScanRunCommand = new CommandBuilder(@"
                                        Update Runs 
                                        SET Status = @Status,
                                            AbortReason = @AbortReason
                                        WHERE RunId = @RunId",

                           new Dictionary<string, Action<DbParameter>>
                           {
                                { "@AbortReason", p => p.DbType = System.Data.DbType.String },
                                { "@Status", p => p.DbType = System.Data.DbType.Int32 },
                                { "@RunId", p => p.DbType = System.Data.DbType.Int32 }
                           });
        #endregion

        #region "AddScanCheckpointGroupCommand"
        public static readonly CommandBuilder AddScanCheckpointGroupCommand = new CommandBuilder(@"
                    INSERT INTO [dbo].[ScanCheckpointGroups] ([ScanId],[CheckpointGroupId]) 
                    VALUES(@ScanId,@CheckpointGroupId)", 
                new Dictionary<string, Action<DbParameter>>
                {
                  { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
                  { "@CheckpointGroupId", p => p.DbType = System.Data.DbType.String }
                });
        #endregion

        #region "AddUserGroupPermissionCommand"
        public static readonly CommandBuilder AddUserGroupPermissionCommand = new CommandBuilder(@"
                    IF NOT EXISTS(SELECT* FROM [dbo].UserGroupPermissions
                                        WHERE UserGroupId = @UserGroupId
                                            AND Type = @UserPermissionType
                                            AND Action = @ActionType
                                            AND TypeId = '*'
                                        UNION
                                        SELECT * FROM [dbo].UserGroupPermissions
                                        WHERE UserGroupId = @UserGroupId 
                                            AND Type = @UserPermissionType
                                            AND Action = @ActionType
                                            AND TypeId = @TypeId)
                            BEGIN
                                INSERT INTO dbo.UserGroupPermissions([UserGroupId],[Type],[TypeId],[Action])
                                VALUES(@UserGroupId, @UserPermissionType, @TypeId, @ActionType)
                            END",
                new Dictionary<string, Action<DbParameter>>
                {
                  { "@UserGroupId", p => p.DbType = System.Data.DbType.Int32 },
                  { "@UserPermissionType", p => p.DbType = System.Data.DbType.String },
                  { "@TypeId", p => p.DbType = System.Data.DbType.String },
                  { "@ActionType", p => p.DbType = System.Data.DbType.String }
                });
        #endregion

        #region "AddScanCheckpointGroupIdCommand"
        public static readonly CommandBuilder AddScanCheckpointGroupIdCommand = new CommandBuilder(@"
                    INSERT INTO [dbo].[ScanCheckpointGroups] ([ScanId],[CheckpointGroupId]) 
                    VALUES(@ScanId, @CheckpointGroupId)",
                new Dictionary<string, Action<DbParameter>>
                {
                  { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
                  { "@CheckpointGroupId", p => p.DbType = System.Data.DbType.String }
                });
        #endregion

        #region "DeleteScanCommands"
        public static readonly CommandBuilder DeleteScanCheckpointGroupsCommand = new CommandBuilder(@"
                    DELETE [dbo].[ScanCheckpointGroups] 
                    WHERE (ScanId = @ScanId)",
                new Dictionary<string, Action<DbParameter>>
                {
                   { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                });
        public static readonly CommandBuilder DeleteScanCheckpointsCommand = new CommandBuilder(@"
                    DELETE [dbo].[ScanCheckpoints] 
                    WHERE (ScanId = @ScanId)",
                new Dictionary<string, Action<DbParameter>>
                {
                   { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                });

        public static readonly CommandBuilder DeleteScanGroupScansCommand = new CommandBuilder(@"
                    DELETE [dbo].[ScanGroupScans] 
                    WHERE (ScanId = @ScanId)",
                 new Dictionary<string, Action<DbParameter>>
                 {
                   { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                 });

        public static readonly CommandBuilder DeleteUserGroupPermissionsCommand = new CommandBuilder(@"
                    DELETE [dbo].[UserGroupPermissions]
                    WHERE (TypeId = CAST(@ScanId AS NVARCHAR(50)))",
                new Dictionary<string, Action<DbParameter>>
                {
                  { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                });

        public static readonly CommandBuilder DeleteResultsCommand = new CommandBuilder(@"
                    DELETE [dbo].[Results]
                    WHERE RunId = @RunId ",
                new Dictionary<string, Action<DbParameter>>
                {
                          { "@RunId", p => p.DbType = System.Data.DbType.Int32 }
                });

        public static readonly CommandBuilder DeleteResultInstancesCommand = new CommandBuilder(@"
                    DELETE [dbo].[ResultInstances]
                    WHERE ResultId in (SELECT ResultId FROM [dbo].[Results] Where RunId = @RunId) ",
                new Dictionary<string, Action<DbParameter>>
                {
                          { "@RunId", p => p.DbType = System.Data.DbType.Int32 }
                });

        public static readonly CommandBuilder DeleteResultViewScanCommand = new CommandBuilder(@"
                    DELETE [dbo].[ResultViewScans]
                    WHERE (ScanId = @ScanId)",
                 new Dictionary<string, Action<DbParameter>>
                 {
                   { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }
                 });

        public static readonly CommandBuilder DeleteScansCommand = new CommandBuilder(@"
                    DELETE [dbo].[Scans] 
                    WHERE (ScanId = @ScanId)",
                new Dictionary<string, Action<DbParameter>>
                {
                  { "@ScanId", p => p.DbType = System.Data.DbType.Int32 }                  
                });
        public static readonly CommandBuilder UpdateRunStatusForDeletedScanCommand = new CommandBuilder(@"
                    UPDATE [dbo].[Runs]
                    SET Status = 8
                    WHERE (RunId = @RunId)",
                new Dictionary<string, Action<DbParameter>>
                {
                  { "@RunId", p => p.DbType = System.Data.DbType.Int32 }
                });
        
        #endregion

        #region "AddScanCommand"
        public static readonly CommandBuilder AddScanCommand = new CommandBuilder(@"
                    DECLARE @seed INTEGER                                       
                    SET IDENTITY_INSERT [dbo].Scans ON
                    IF (SELECT ISNULL(Max(ScanId),0) FROM [dbo].Scans) > 0
                        BEGIN                                                                               
                            SELECT @seed = MAX(ScanId) - 1998 FROM [dbo].Scans WHERE ScanId < 9000 
                            DBCC CHECKIDENT ('[dbo].Scans', RESEED, @seed)                        
                        END
                    ELSE
                        SET @seed = 1
                    INSERT INTO [dbo].[Scans]
                                ([ScanId]
                                ,[IsMonitor]
                                ,[BaseUrl]
                                ,[DisplayName]
                                ,[IncludedDomains]
                                ,[IncludeFilter]
                                ,[ExcludeFilter]
                                ,[UserAgent]
                                ,[IncludeMSOfficeDocs]
                                ,[IncludeOtherDocs]
                                ,[RetestAllPages]
                                ,[AlertMode]
                                ,[AlertDelay]
                                ,[AlertSendTo]
                                ,[AlertSubject]
                                ,[StartPages]
                                ,[DateCreated]
                                ,[DateModified]
                                ,[LocalClientId])
                            VALUES
                               ((SELECT @seed + IDENT_INCR ('[dbo].Scans')),
                                @IsMonitor,
                                @BaseUrl,
                                @DisplayName,
                                @IncludedDomains,
                                @IncludeFilter,
                                @ExcludeFilter,
                                @UserAgent,
                                @IncludeMSOfficeDocs,
                                @IncludeOtherDocs,
                                @RetestAllPages,
                                @AlertMode,
                                @AlertDelay,
                                @AlertSendTo,
                                @AlertSubject,
                                @StartPages,
                                @DateCreated,
                                @DateModified,
                                @LocalClientId)                            
                            SET @AddedScanId = SCOPE_IDENTITY()
                            
                            IF (SELECT columnproperty(object_id('Scans'),'ScanId','IsIdentity')) = '1'
                                SET IDENTITY_INSERT [dbo].Scans OFF                           
                             ",
                      new Dictionary<string, Action<DbParameter>>
                      {                        
                        { "@IsMonitor", p => p.DbType = System.Data.DbType.Boolean },
                        { "@BaseUrl", p => p.DbType = System.Data.DbType.String },
                        { "@DisplayName", p => p.DbType = System.Data.DbType.String },
                        { "@IncludedDomains", p => p.DbType = System.Data.DbType.String },
                        { "@IncludeFilter", p => p.DbType = System.Data.DbType.String },
                        { "@ExcludeFilter", p => p.DbType = System.Data.DbType.String },
                        { "@UserAgent", p => p.DbType = System.Data.DbType.String },
                        { "@IncludeMSOfficeDocs", p => p.DbType = System.Data.DbType.Boolean },
                        { "@IncludeOtherDocs", p => p.DbType = System.Data.DbType.Boolean },
                        { "@RetestAllPages", p => p.DbType = System.Data.DbType.Boolean },
                        { "@AlertMode", p => p.DbType = System.Data.DbType.Int32 },
                        { "@AlertDelay", p => p.DbType = System.Data.DbType.Int32 },
                        { "@AlertSendTo", p => p.DbType = System.Data.DbType.String },
                        { "@AlertSubject", p => p.DbType = System.Data.DbType.String },
                        { "@StartPages", p => p.DbType = System.Data.DbType.String },
                        { "@DateCreated", p => p.DbType = System.Data.DbType.DateTime },
                        { "@DateModified", p => p.DbType = System.Data.DbType.DateTime },
                        { "@LocalClientId", p => p.DbType = System.Data.DbType.String },                        
                        { "@AddedScanId", p => { p.DbType = System.Data.DbType.Int32; p.Direction = System.Data.ParameterDirection.Output; } },
                      }
                  );
        #endregion

        #region "UpdateScanCommand"
        public static readonly CommandBuilder UpdateScanCommand = new CommandBuilder(@"
                         UPDATE [dbo].[Scans]
                         SET [IsMonitor] = @IsMonitor
                                ,[BaseUrl] = @BaseUrl
                                ,[DisplayName] = @DisplayName
                                ,[IncludedDomains] = @IncludedDomains
                                ,[IncludeFilter] = @IncludeFilter
                                ,[ExcludeFilter] = @ExcludeFilter
                                ,[UserAgent] = @UserAgent
                                ,[IncludeMSOfficeDocs] = @IncludeMSOfficeDocs
                                ,[IncludeOtherDocs] = @IncludeOtherDocs
                                ,[RetestAllPages] = @RetestAllPages
                                ,[AlertMode] = @AlertMode
                                ,[AlertDelay] = @AlertDelay
                                ,[AlertSendTo] = @AlertSendTo
                                ,[AlertSubject] = @AlertSubject
                                ,[StartPages] = @StartPages
                                ,[DateModified] = @DateModified
                                ,[LocalClientId] = @LocalClientId
                         WHERE ScanId = @ScanId",
                     new Dictionary<string, Action<DbParameter>>
                     {
                        { "@ScanId", p => p.DbType = System.Data.DbType.Int32 },
                        { "@IsMonitor", p => p.DbType = System.Data.DbType.Boolean },
                        { "@BaseUrl", p => p.DbType = System.Data.DbType.String },
                        { "@DisplayName", p => p.DbType = System.Data.DbType.String },
                        { "@IncludedDomains", p => p.DbType = System.Data.DbType.String },
                        { "@IncludeFilter", p => p.DbType = System.Data.DbType.String },
                        { "@ExcludeFilter", p => p.DbType = System.Data.DbType.String },
                        { "@UserAgent", p => p.DbType = System.Data.DbType.String },
                        { "@IncludeMSOfficeDocs", p => p.DbType = System.Data.DbType.Boolean },
                        { "@IncludeOtherDocs", p => p.DbType = System.Data.DbType.Boolean },
                        { "@RetestAllPages", p => p.DbType = System.Data.DbType.Boolean },
                        { "@AlertMode", p => p.DbType = System.Data.DbType.Int32 },
                        { "@AlertDelay", p => p.DbType = System.Data.DbType.Int32 },
                        { "@AlertSendTo", p => p.DbType = System.Data.DbType.String },
                        { "@AlertSubject", p => p.DbType = System.Data.DbType.String },
                        { "@StartPages", p => p.DbType = System.Data.DbType.String },
                        { "@DateModified", p => p.DbType = System.Data.DbType.DateTime },
                        { "@LocalClientId", p => p.DbType = System.Data.DbType.String }
                     }
                 );
        #endregion

        #endregion

        private readonly ConfigurationOptions _options;
        private readonly ILogger<ScanMutator> _logger;
        private readonly ILogMessagesMutator _logMessagesMutator;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IFileSystemService _fileSystemService;

        public ScanMutator(ILogger<ScanMutator> logger, 
                           ILogMessagesMutator logMessagesMutator,
                           IUnitOfWorkFactory unitOfWorkFactory,
                           IOptions<ConfigurationOptions> options,
                           IFileSystemService fileSystemService)
        {
            this._logger = logger;
            this._options = options.Value;
            this._logMessagesMutator = logMessagesMutator;
            this._unitOfWorkFactory = unitOfWorkFactory;
            this._fileSystemService = fileSystemService;
        }

        public async Task<bool> DeleteScanAndDependencies(int scanId, int lastCompletedRunId)
        {
            bool delStatus = false;
            try
            {
                using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
                {
                    DeleteScanCheckpoints(scanId, unitOfWork);
                    DeleteScanCheckpointGroups(scanId, unitOfWork);
                    DeleteScanGroupScans(scanId, unitOfWork);
                    DeleteUserGroupPermissions(scanId, unitOfWork);
                    DeleteResultInstances(lastCompletedRunId, unitOfWork);
                    DeleteResults(lastCompletedRunId, unitOfWork);
                    DeleteResultViewScan(scanId, unitOfWork);
                    DeleteScan(scanId, unitOfWork);
                    UpdateRunStatusForDeletedScan(lastCompletedRunId, unitOfWork);
                    await unitOfWork.CommitAsync(CancellationToken.None);
                }
                delStatus = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteScanAndDependencies - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
            return delStatus;
        }
        public void DeleteScanCheckpointGroups(int scanId,IUnitOfWork unitOfWork)
        {
            try
            {               
                    unitOfWork.DeferSql(async (connection, cancellationToken) =>
                    {
                        using (var command = await DeleteScanCheckpointGroupsCommand.BuildFrom(connection, new Dictionary<string, object>
                        {
                        {"@ScanId",scanId }
                        }, cancellationToken))
                        {
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                    });      
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteScanCheckpointGroups - Error Msg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }

        public void DeleteScanCheckpoints(int scanId, IUnitOfWork unitOfWork)
        {
            try
            {                
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await DeleteScanCheckpointsCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                        {"@ScanId",scanId }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteScanCheckpoints - Error Msg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }

        public void DeleteScanGroupScans(int scanId, IUnitOfWork unitOfWork)
        {
            try
            {                
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await DeleteScanGroupScansCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                    {"@ScanId",scanId }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteScanGroupScans - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
}

        public void DeleteUserGroupPermissions(int scanId, IUnitOfWork unitOfWork)
        {
            try
            {               
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await DeleteUserGroupPermissionsCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                    {"@ScanId",scanId }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteUserGroupPermissions - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }
        public void DeleteResults(int runId, IUnitOfWork unitOfWork)
        {
            try
            {               
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await DeleteResultsCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                    {"@RunId",runId }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteResults - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }

        public void DeleteResultInstances(int runId, IUnitOfWork unitOfWork)
        {
            try
            {               
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await DeleteResultInstancesCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                    {"@RunId",runId }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteResultInstances - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }
        public void DeleteResultViewScan(int scanId, IUnitOfWork unitOfWork)
        {
            try
            {                
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await DeleteResultViewScanCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                    {"@ScanId",scanId }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });                
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteResultViewScan - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }
        
        public void UpdateRunStatusForDeletedScan(int runId, IUnitOfWork unitOfWork)
        {
            try
            {
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await UpdateRunStatusForDeletedScanCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                    {"@RunId",runId }
                    }, cancellationToken))
                    {                        
                       await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });                
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateRunStatusForDeletedScan - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }
        public void DeleteScan(int scanId, IUnitOfWork unitOfWork)
        {
            try
            {
                 unitOfWork.DeferSql(async (connection, cancellationToken) =>
                 {
                    using (var command = await DeleteScansCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                    {"@ScanId",scanId }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateRunStatusForDeletedScan - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<bool> UpdateScanAndCheckpointGroups(int scanId, ScanUpdateRequest request)
        {
            bool editScanStatus = false;
            try
            {
                using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
                {
                    UpdateScan(request, unitOfWork);
                    DeleteScanCheckpointGroups(scanId, unitOfWork);
                    AddScanCheckpointGroupIds(scanId, request.CheckpointGroupIds, unitOfWork);
                    await unitOfWork.CommitAsync(CancellationToken.None);
                    editScanStatus = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateScanAndCheckpointGroups - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
            return editScanStatus;
        }
        public void UpdateScan(ScanUpdateRequest request,IUnitOfWork unitOfWork)
        {
            try
            {                
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    using (var command = await UpdateScanCommand.BuildFrom(connection, new Dictionary<string, object>
                    {
                    { "@ScanId",request.ScanId },
                    { "@IsMonitor", request.IsMonitor },
                    { "@BaseUrl", request.BaseUrl },
                    { "@DisplayName", request.DisplayName },
                    { "@IncludedDomains", request.IncludedDomains.Replace(" ", Environment.NewLine) },
                    { "@IncludeFilter", request.IncludeFilter },
                    { "@ExcludeFilter", request.ExcludeFilter },
                    { "@UserAgent", request.UserAgent },
                    { "@IncludeMSOfficeDocs", Convert.ToBoolean(request.IncludeMsofficeDocs) },
                    { "@IncludeOtherDocs", Convert.ToBoolean(request.IncludeOtherDocs) },
                    { "@RetestAllPages", Convert.ToBoolean(request.RetestAllPages) },
                    { "@AlertMode", request.AlertMode },
                    { "@AlertDelay", request.AlertDelay },
                    { "@AlertSendTo", request.AlertSendTo },
                    { "@AlertSubject", request.AlertSubject },
                    { "@StartPages", Helpers.Helper.ConvertObjectToXmlString(request.StartPages) },
                    { "@DateModified", request.DateModified.Value.DateTime },
                    { "@LocalClientId", request.LocalClientId }
                    }, cancellationToken))
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                });
                    
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateScan - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }

        public void AddScanCheckpointGroupIds(int scanId, List<string> checkPointGroupIds, IUnitOfWork unitOfWork)
        {
            try
            {               
                unitOfWork.DeferSql(async (connection, cancellationToken) =>
                {
                    foreach (string chkGroupId in checkPointGroupIds)
                    {
                        using (var command = await AddScanCheckpointGroupIdCommand.BuildFrom(connection, new Dictionary<string, object>
                        {
                        {"@ScanId",scanId },
                        {"@CheckpointGroupId",chkGroupId}
                        }, cancellationToken))
                        {
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                    }
                });
                _logger.LogInformation("The ScanCheckpointGroup for scanId {0} successfully added in [ScanCheckpointGroups] table.", scanId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError("AddScanCheckpointGroupIds - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<int> InsertScanAndDependencies(ScanRequest request, int userGroupId, string userPermissionType, string actionType)
        {            
            int newScanId = 0;
            try
            {
                using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
                {
                    //Add Scan into Scans Table.
                    unitOfWork.DeferSql(async (connection, cancellationToken) =>
                    {
                        using (var command = await AddScanCommand.BuildFrom(connection, new Dictionary<string, object>
                        {
                        { "@IsMonitor", request.IsMonitor },
                        { "@BaseUrl", request.BaseUrl },
                        { "@DisplayName", request.DisplayName },
                        { "@IncludedDomains", request.IncludedDomains },
                        { "@IncludeFilter", request.IncludeFilter },
                        { "@ExcludeFilter", request.ExcludeFilter },
                        { "@UserAgent", request.UserAgent },
                        { "@IncludeMSOfficeDocs", Convert.ToBoolean(request.IncludeMsofficeDocs) },
                        { "@IncludeOtherDocs", Convert.ToBoolean(request.IncludeOtherDocs) },
                        { "@RetestAllPages", Convert.ToBoolean(request.RetestAllPages) },
                        { "@AlertMode", request.AlertMode },
                        { "@AlertDelay", request.AlertDelay },
                        { "@AlertSendTo", request.AlertSendTo },
                        { "@AlertSubject", request.AlertSubject },
                        { "@StartPages", Helpers.Helper.ConvertObjectToXmlString(request.StartPages) },
                        { "@DateCreated", request.DateCreated.Value.DateTime },
                        { "@DateModified", request.DateModified.Value.DateTime },
                        { "@LocalClientId", request.LocalClientId }                        
                        }, cancellationToken))
                        {
                            await command.ExecuteNonQueryAsync(cancellationToken);
                            newScanId = Convert.ToInt32(command.Parameters["@AddedScanId"].Value.ToString());
                        }
                    });

                    //Add Scan And CheckpointGroupId into ScanCheckpointGroups Table.
                    unitOfWork.DeferSql(async (connection, cancellationToken) =>
                    {
                        foreach (string chkPoint in request.CheckpointGroupIds)
                        {
                            using (var commandCheckpointGroup = await AddScanCheckpointGroupCommand.BuildFrom(connection, new Dictionary<string, object>
                            {
                                { "@ScanId", newScanId },
                                { "@CheckpointGroupId", chkPoint}
                            }, cancellationToken))
                            {
                                await commandCheckpointGroup.ExecuteNonQueryAsync(cancellationToken);
                            }
                        }
                    });

                    //Check existing permission for UserGroup and if required add ScanId with "edit" permission into UsergroupPermissions Table.
                    unitOfWork.DeferSql(async (connection, cancellationToken) =>
                    {
                        using (var commandUserGroupPermission = await AddUserGroupPermissionCommand.BuildFrom(connection, new Dictionary<string, object>
                        {
                        { "@UserGroupId", userGroupId },
                        { "@UserPermissionType", userPermissionType},
                        { "@TypeId", newScanId },
                        { "@ActionType",actionType }
                        }, cancellationToken))
                        {                            
                           await commandUserGroupPermission.ExecuteNonQueryAsync(cancellationToken);
                        }
                    });                    
                    await unitOfWork.CommitAsync(CancellationToken.None);
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError("InsertScanAndDependencies - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
            return newScanId;
        }      

        public async Task<int> AddScanRun(int scanId, string orgId, int? scanGroupId, int? scanGroupRunId, bool scheduledScan)
        {
            int newRunId = 0;
            try
            {
                using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
                {
                    unitOfWork.DeferSql(async (connection, cancellationToken) =>
                    {
                        using (var command = await AddScanRunCommand.BuildFrom(connection, new Dictionary<string, object>
                        {
                        { "@ScanId", scanId },
                        { "@Status", 4 },
                        { "@Started", DateTime.UtcNow },
                        { "@ScheduledScan", scheduledScan },
                        { "@ScanGroupId", scanGroupId == null ? (object)DBNull.Value : scanGroupId },
                        { "@ScanGroupRunId", scanGroupRunId == null ? (object)DBNull.Value : scanGroupRunId }
                        }, cancellationToken))
                        {
                            await command.ExecuteNonQueryAsync(cancellationToken);
                            newRunId = Convert.ToInt32(command.Parameters["@ID"].Value.ToString());
                        }
                    });

                    await unitOfWork.CommitAsync(CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AddScanRun - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
            return newRunId;
        }

        public async Task UpdateScanRun(int runId, string taskId, string orgId)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
                {
                    unitOfWork.DeferSql(async (connection, cancellationToken) =>
                    {
                        using (var command = await UpdateScanRunCommand.BuildFrom(connection, new Dictionary<string, object>
                        {
                        { "@TaskId", taskId },
                        { "@RunId", runId },
                        }, cancellationToken))
                        {
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                    });

                    await unitOfWork.CommitAsync(CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateScanRun - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task AbortScanRun(int runId, int status, string abortReason)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork())
                {
                    unitOfWork.DeferSql(async (connection, cancellationToken) =>
                    {
                        using (var command = await AbortScanRunCommand.BuildFrom(connection, new Dictionary<string, object>
                        {
                        { "@AbortReason", abortReason },
                        { "@Status", status },
                        { "@RunId", runId },
                        }, cancellationToken))
                        {
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                    });

                    await unitOfWork.CommitAsync(CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AbortScanRun - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }

        }
        public bool DeleteScanResultFiles(int runId, string organizationId, CancellationToken cancellationToken)
        {
            bool status = false;
            try
            {
                var customerFolder = _fileSystemService.GetCustomerFolder(organizationId);
                var scanRunPath = Path.Combine(customerFolder, "saved\\" + runId.ToString());
                _logger.LogInformation("Scan Run Path :" + scanRunPath);
                System.IO.DirectoryInfo di = new DirectoryInfo(scanRunPath);
                if (di.Exists)
                {
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                    status = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteScanResultFiles - ErrorMsg:" + ex.Message + Environment.NewLine + "StackTrace:" + ex.StackTrace);
                throw ex;
            }
            return status;
        }
    }
}
