using System;
using System.Threading;
using System.Threading.Tasks;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.Work;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace ComplianceSheriff.Permission
{
    public class PermissionService : IPermissionService
    {
        private readonly JwtSignInHandler _jwtSignInHandler;
        private readonly IPermissionMutator _userPermissionMutator;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService([FromServices] JwtSignInHandler jwtSignInHandler,
                                 [FromServices] IPermissionMutator userPermissionMutator,
                                 [FromServices] ILogger<PermissionService> logger)
        {
            _jwtSignInHandler = jwtSignInHandler;
            _userPermissionMutator = userPermissionMutator;
            _logger = logger;
        }

        public async Task RecordUserPermission(Int32 userGroupId,
                                           string userPermissionType,
                                           string typeId,
                                           string actionType,
                                           IUnitOfWork unitOfWork)
        {

            var userGroupPermission = BuildUserAuditPermission(userGroupId,userPermissionType,typeId,actionType);
            await CreateUserGroupPermissionRecord(userGroupPermission, unitOfWork);
            _logger.LogInformation("The permission {0} for object type {1} and Id : {2} is successfully added in [UserGroupPermissions] table for UserGroupId : {3}.", actionType, userPermissionType, typeId,userGroupId);
        }

        public async Task CreateUserGroupPermissionRecord(UserGroupPermission userPermission, IUnitOfWork unitOfWork)
        {
                _userPermissionMutator.AddUserGroupPermissionRecord(userPermission, unitOfWork);
                await unitOfWork.CommitAsync(CancellationToken.None);
        }

        public UserGroupPermission BuildUserAuditPermission(Int32 userGroupId,
                                           string userPermissionType,
                                           string typeId,
                                           string actionType)
        {
            var userGroupPermission = new UserGroupPermission
            {
                UserGroupId = userGroupId,
                Type = userPermissionType,
                TypeId = typeId,
                Action = actionType
            };

            return userGroupPermission;
        }
    }
}
