using ComplianceSheriff.Permission;
using ComplianceSheriff.UsageAudit;
using ComplianceSheriff.UserGroups;
using ComplianceSheriff.Work;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.ScanGroups
{
    public class ScanGroupService : IScanGroupService
    {
        private readonly IUserGroupMutator _userGroupMutator;
        public readonly IScanGroupAccessor _scanGroupAccessor;
        private readonly IScanGroupMutator _scanGroupMutator;
        private readonly IPermissionAccessor _permissionAccessor;
        private readonly IPermissionMutator _permissionMutator;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IUsageAuditService _usageAuditService;
        private readonly ILogger<ScanGroupService> _logger;

        public ScanGroupService([FromServices] IScanGroupMutator scanGroupMutator,
                                [FromServices] IScanGroupAccessor scanGroupAccessor,
                                [FromServices] IUserGroupMutator userGroupMutator,
                                [FromServices] IPermissionAccessor permissionAccessor,
                                [FromServices] IPermissionMutator permissionMutator,
                                [FromServices] IUsageAuditService usageAuditService,
                                [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                [FromServices] ILogger<ScanGroupService> logger)
        {
            this._scanGroupAccessor = scanGroupAccessor;
            this._userGroupMutator = userGroupMutator;
            this._scanGroupMutator = scanGroupMutator;
            this._usageAuditService = usageAuditService;
            this._permissionAccessor = permissionAccessor;
            this._permissionMutator = permissionMutator;
            this._unitOfWorkFactory = unitOfWorkFactory;
            this._logger = logger;
        }

        public async Task UpdateScanGroupName(int scanGroupId, string scanGroupName, HttpContext context, CancellationToken cancellationToken)
        {            
            using (var unitOfWork = this._unitOfWorkFactory.CreateUnitOfWork())
            {
                this._scanGroupMutator.UpdateScanGroupName(scanGroupId, scanGroupName, unitOfWork);

                _usageAuditService.RecordUserAction($"Display Name : '{scanGroupName}'",
                                                    scanGroupName,
                                                    "New Scan Group Created",
                                                    UserAuditType.ScanGroups,
                                                    UserAuditActionType.Modify,
                                                    context,
                                                    unitOfWork);

                await unitOfWork.CommitAsync(cancellationToken);
            }
        }

        public async Task<int> CreateScanGroup(string scanGroupName, int userGroupId, HttpContext context, bool setAsDefault, CancellationToken cancellationToken)
        {
            var scanGroupId = await this._scanGroupMutator.AddScanGroup(scanGroupName);
            
            var hasEditPermission = await this._permissionAccessor.CheckPermissionTypeAndActionForUserGroup(scanGroupId, userGroupId, typeof(ScanGroup).Name, "edit", cancellationToken);

            if (!hasEditPermission)
            {
                var userGroupPermission = new UserGroupPermission
                {
                    Action = "edit",
                    Type = "ScanGroup",
                    TypeId = scanGroupId.ToString(),
                    UserGroupId = userGroupId
                };

                try
                {
                    using (var unitOfWork = this._unitOfWorkFactory.CreateUnitOfWork())
                    {
                        this._permissionMutator.AddUserGroupPermissionRecord(userGroupPermission, unitOfWork);
                        await unitOfWork.CommitAsync(CancellationToken.None);
                    }

                    using (var unitOfWork = this._unitOfWorkFactory.CreateUnitOfWork())
                    {
                        _usageAuditService.RecordUserAction($"Display Name : '{scanGroupName}'",
                                                            scanGroupName,
                                                            "New Scan Group Created",
                                                            UserAuditType.ScanGroups,
                                                            UserAuditActionType.Create,
                                                            context,
                                                            unitOfWork);

                        await unitOfWork.CommitAsync(cancellationToken);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError($"There was an error adding ScanGroup permissions for ScanGroup {scanGroupId} to UserGroup {userGroupId} " + Environment.NewLine + ex.Message);
                }
            } else
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append($"ScanGroupId {scanGroupId} was not given permission to UserGroupId {userGroupId} ");
                stringBuilder.Append($"because UserGroup {userGroupId} already has edit permissions for ScanGroup {scanGroupId}");
                _logger.LogInformation(stringBuilder.ToString());
            }

            if (setAsDefault)
            {
                await this._userGroupMutator.UpdateScanGroupId(userGroupId, scanGroupId);
            }

            return scanGroupId;
        }

        public async Task<ScanGroup> GetScanGroupById(int scanGroupId, CancellationToken cancellationToken)
        {
            var scanGroup = await this._scanGroupAccessor.GetScanGroupByScanGroupId(scanGroupId, cancellationToken);
            return scanGroup;
        }
    }
}
