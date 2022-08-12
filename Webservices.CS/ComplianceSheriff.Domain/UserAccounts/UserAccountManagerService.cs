using ComplianceSheriff.Enums;
using ComplianceSheriff.Passwords;
using ComplianceSheriff.Permission;
using ComplianceSheriff.Responses;
using ComplianceSheriff.UserGroups;
using ComplianceSheriff.UserInfos;
using ComplianceSheriff.UserMapping;
using ComplianceSheriff.Users;
using ComplianceSheriff.Work;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserAccounts
{
    public class UserAccountManagerService : IUserAccountManagerService
    {
        private readonly ILogger _logger;
        private readonly IPasswordService _passwordService;
        private readonly IUserAccessor _userAccessor;
        private readonly IUserMutator _userMutator;
        private readonly IUserInfoAccessor _userInfoAccessor;
        private readonly IUserInfoMutator _userInfoMutator;
        private readonly IUserGroupAccessor _userGroupAccessor;
        private readonly IUserGroupMutator _userGroupMutator;
        private readonly IUserMapperAccessor _userMapperAccessor;
        private UserAccountManagerResponse response = new UserAccountManagerResponse();


        public UserAccountManagerService(IUserAccessor userAccessor,
                                         IUserMapperAccessor userMapperAccessor,
                                         IUserMutator userMutator,
                                         IUserInfoAccessor userInfoAccessor,
                                         IUserInfoMutator userInfoMutator,
                                         IPasswordService passwordService,
                                         ILogger<UserAccountManagerService> logger,
                                         IUserGroupMutator userGroupMutator,
                                         IUserGroupAccessor userGroupAccessor)
        {
            this._userAccessor = userAccessor;
            this._userMutator = userMutator;
            this._userInfoAccessor = userInfoAccessor;
            this._userInfoMutator = userInfoMutator;
            this._passwordService = passwordService;
            this._userGroupMutator = userGroupMutator;
            this._userGroupAccessor = userGroupAccessor;
            this._userMapperAccessor = userMapperAccessor;
            this._logger = logger;
        }


        public async Task<UserResponse> GetUserByUserName(string userName, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _userAccessor.GetUserByUserName(userName, cancellationToken);

                if (result != null)
                {
                    return new UserResponse
                    {
                        UserId = result.UserId,
                        Name = result.Name,
                        FirstName = result.FirstName,
                        LastName = result.LastName,
                        EmailAddress = result.EmailAddress,
                        OrganizationId = result.OrganizationId,
                        TimeZone = result.TimeZone,
                    };
                } else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public async Task<UserAccountManagerResponse> CheckUser(User user, CancellationToken cancellationToken)
        {
            try
            {            
                var result = await _userAccessor.GetUserByUserName(user.Name, cancellationToken);
                if (result != null)
                {
                    return new UserAccountManagerResponse { Status = UserAccountCreateStatus.UserExists };
                } else 
                {
                    return new UserAccountManagerResponse { Status = UserAccountCreateStatus.UserDoesNotExist };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public async Task<UserAccountManagerResponse> CheckUserGroup(string userGroupName, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _userGroupAccessor.GetUserGroupByUserGroupName(userGroupName, cancellationToken);
                if (result != null)
                {
                    return new UserAccountManagerResponse { Status = UserAccountCreateStatus.UserExists };
                }
                else
                {
                    return new UserAccountManagerResponse { Status = UserAccountCreateStatus.UserDoesNotExist };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public async Task<UserAccountManagerResponse> CheckUserMapper(User user, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _userMapperAccessor.GetUserMapperByOrgUserId(user.UserId, cancellationToken);
                if (result != null)
                {
                    return new UserAccountManagerResponse { Status = UserAccountCreateStatus.UserProfileExists };
                }
                else
                {
                    return new UserAccountManagerResponse { Status = UserAccountCreateStatus.UserProfileDoesNotExist };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public async Task<UserAccountManagerResponse> CreateOrgUser(User user, CancellationToken cancellationToken)
        {         
            try
            {
                var _user = await this._userAccessor.GetUserByUserName(user.Name, cancellationToken);

                if (_user != null)
                {
                    response.Status = UserAccountCreateStatus.UserExists;
                }
                else
                {
                    user.Password = _passwordService.EncryptPassword(user.Password);
                    response.ReturnId = await _userMutator.AddUser(user);
                    response.Status = UserAccountCreateStatus.UserCreated;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public async Task<UserAccountManagerResponse> UpdateUserName(int userId, string userName, CancellationToken cancellationToken)
        {           
            var response = new UserAccountManagerResponse();
            await Task.Delay(1000);
            return response;
        }

        public async Task<UserAccountManagerResponse> UpdateUser(User currentUser, User userToUpdate, IUnitOfWorkFactory unitOfWorkFactory, CancellationToken cancellationToken)
        {
            try
            {
                if (currentUser.Name != userToUpdate.Name)
                {
                    //Verify that the UserName does not exist
                    var existingUser = await _userAccessor.GetUserByUserName(userToUpdate.Name, cancellationToken);

                    if (existingUser != null)
                    {
                        response.Status = UserAccountCreateStatus.UserExists;
                        return response;
                    }

                    //Update UserName in the Users table
                    await _userMutator.UpdateUserName(userToUpdate.Name, userToUpdate.UserId);

                    //Retrieve data from UserInfos table
                    var userInfo = await _userInfoAccessor.GetUserInfoByUserInfoId(currentUser.Name, cancellationToken);

                    //Insert New Record in UserInfo table with updated UserInfoId
                    userInfo.UserInfoId = userToUpdate.Name;
                    await _userInfoMutator.AddUserInfo(userInfo, cancellationToken);

                    //Delete the original userInfo row in the UserInfos table
                    await _userInfoMutator.RemoveUserInfo(currentUser.Name, cancellationToken);
                }

                var userInfoRecord = await _userInfoAccessor.GetUserInfoByUserInfoId(userToUpdate.Name, cancellationToken);
                userInfoRecord.EmailAddress = userToUpdate.EmailAddress;
                userInfoRecord.TimeZone = userToUpdate.TimeZone;
                
                var user = await _userAccessor.GetUserByUserId(userToUpdate.UserId, cancellationToken);
                user.FirstName = userToUpdate.FirstName;
                user.LastName = userToUpdate.LastName;

                using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                {
                    _userMutator.UpdateUser(user, unitOfWork);
                    _userInfoMutator.UpdateUserInfo(userInfoRecord, unitOfWork, cancellationToken);

                    await unitOfWork.CommitAsync(CancellationToken.None);
                }

                response.Status = UserAccountCreateStatus.UserUpdated;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public async Task<UserAccountManagerResponse> CreateOrgInfos(UserInfo userInfo, CancellationToken cancellationToken)
        {
            try
            {
                await _userInfoMutator.AddUserInfo(userInfo, cancellationToken);
                response.Status = UserAccountCreateStatus.UserInfoCreated;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public async Task<UserAccountManagerResponse> CreateUserMapping(User user, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
        {
            try
            {

                //Check if User Exists in UserMapper table
                var userProfile = await this._userMapperAccessor.GetUserMapperByOrgUserId(user.UserId, cancellationToken);

                //Insert new UserProfile record
                if (userProfile != null)
                {
                    response.Status = UserAccountCreateStatus.UserProfileExists;
                }
                else
                {
                    _userMutator.AddUserMapper(user, unitOfWork);
                    response.Status = UserAccountCreateStatus.UserProfileCreated;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public async Task<UserAccountManagerResponse> CreateUserGroup(string userGroupName, CancellationToken cancellationToken)
        {
            try
            {
                //Check if UserGroup Exists in Org UserGroups table
                var userGroup = await this._userGroupAccessor.GetUserGroupByUserGroupName(userGroupName, cancellationToken);

                //Insert new UserProfile record
                if (userGroup != null)
                {
                    response.Status = UserAccountCreateStatus.UserGroupExists;
                }
                else
                {
                    response.ReturnId = await _userGroupMutator.AddUserGroup(userGroupName);
                    response.Status = UserAccountCreateStatus.UserGroupCreated;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        public async Task<UserAccountManagerResponse> CreateUserUserGroupAndUserMapping(User user, UserInfo userInfo, IUnitOfWorkFactory unitOfWorkFactory, IPermissionService permissionService, CancellationToken cancellationToken)
        {
            try
            {
                //Check for existence of User
                var checkUserResponse = await CheckUser(user, cancellationToken);
                if (checkUserResponse.Status == UserAccountCreateStatus.UserExists)
                {
                    return checkUserResponse;
                }

                //Check for existence of UserGroup
                var checkUserGroupResponse = await CheckUserGroup(user.UserGroupName, cancellationToken);
                if (checkUserGroupResponse.Status == UserAccountCreateStatus.UserGroupExists)
                {
                    return checkUserGroupResponse;
                }

                //Create UserGroup
                response = await CreateUserGroup(user.UserGroupName, cancellationToken);
               
                if (response.Status == UserAccountCreateStatus.UserGroupExists)
                {
                    return response;
                };                

                //Set UserGroupId
                user.UserGroupId = response.ReturnId;

                //Create UserGroupPermissions
                using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                {
                    var checkpointsPermission = new UserGroupPermission { UserGroupId = response.ReturnId, Type = "Checkpoint", TypeId = "*", Action = "view" };
                    var checkpointGroupsPermission = new UserGroupPermission { UserGroupId = response.ReturnId, Type = "CheckpointGroup", TypeId = "*", Action = "view" };
                    var scanCreatePermission = new UserGroupPermission { UserGroupId = response.ReturnId, Type = "Scan", TypeId = "*", Action = "create" };
                    var issueTrackerViewPermission = new UserGroupPermission { UserGroupId = response.ReturnId, Type = "IssueTrackerReport", TypeId = "*", Action = "view" };
                    var scanGroupPermission = new UserGroupPermission { UserGroupId = response.ReturnId, Type = "ScanGroup", TypeId = "*", Action = "create" };

                    await permissionService.CreateUserGroupPermissionRecord(checkpointsPermission, unitOfWork);
                    await permissionService.CreateUserGroupPermissionRecord(checkpointGroupsPermission, unitOfWork);
                    await permissionService.CreateUserGroupPermissionRecord(scanCreatePermission, unitOfWork);
                    await permissionService.CreateUserGroupPermissionRecord(issueTrackerViewPermission, unitOfWork);
                    await permissionService.CreateUserGroupPermissionRecord(scanGroupPermission, unitOfWork);
                }

                //Create Organization User
                response = await CreateOrgUser(user, cancellationToken);
                if (response.Status == UserAccountCreateStatus.UserExists)
                {
                    return response;
                };

                //Create Organization UserInfo record
                await CreateOrgInfos(userInfo, cancellationToken);

                //Create User Mapping
                using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                {
                    //Set userOrgId
                    user.OrgUserId = response.ReturnId;

                    response = await CreateUserMapping(user, unitOfWork, cancellationToken);
                    if (response.Status == UserAccountCreateStatus.UserProfileExists)
                    {
                        unitOfWork.Dispose();
                        return response;
                    }

                    await unitOfWork.CommitAsync(CancellationToken.None);
                }

                response.Status = UserAccountCreateStatus.UserAndProfileCreated;
                return response;
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError($"{ex.Message} - {ex.StackTrace}");

                //rollback/undo all updates
                RollBackCreateUser(user, userInfo, permissionService, cancellationToken);
                throw;
            }
        }

        public async Task<PasswordResetResult> SetTempPasswordAndVerificationToken(User user, CancellationToken cancellationToken)
        {
            try
            {
                var tempPassword = _passwordService.GenerateRandomPassword();
                var hashResult = _passwordService.GenerateHash(tempPassword);
                var encryptedTempPassword = _passwordService.EncryptPassword(tempPassword);

                await _userMutator.SetTempPasswordAndVerificationToken(user, encryptedTempPassword, hashResult);

                return new PasswordResetResult { TempPassword = tempPassword, HashResult = hashResult };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
        }

        public async Task ResetUserPassword(int userId, string password, CancellationToken cancellationToken)
        {
            try
            {
                var encryptedPassword = _passwordService.EncryptPassword(password);

                await _userMutator.ResetUserPassword(userId, encryptedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
        }

        public async Task UpdateUserPassword(int userId, string password, CancellationToken cancellationToken)
        {
            try
            {
                var encryptedPassword = _passwordService.EncryptPassword(password);

                await _userMutator.UpdateUserPassword(userId, encryptedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
        }

        private void RollBackCreateUser(User user, UserInfo userInfo, IPermissionService permissionService, CancellationToken cancellationToken)
        {
            //Remove Permissions
            permissionService.DeleteUserGroupPermissionByUserGroupId(user.UserGroupId);
            _userGroupMutator.DeleteUserGroupById(user.UserGroupId);
            _userInfoMutator.RemoveUserInfo(userInfo.UserInfoId, cancellationToken);
            _userMutator.RemoveUserMapper(user);
            _userMutator.RemoveUser(user);
        }
    }
}
