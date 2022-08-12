using ComplianceSheriff.Enums;
using ComplianceSheriff.Passwords;
using ComplianceSheriff.Permission;
using ComplianceSheriff.Responses;
using ComplianceSheriff.UserInfos;
using ComplianceSheriff.Users;
using ComplianceSheriff.Work;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserAccounts
{
    public interface IUserAccountManagerService
    {
        Task<UserResponse> GetUserByUserName(string userName, CancellationToken cancellationToken);
        Task<UserAccountManagerResponse> CreateOrgUser(User user, CancellationToken cancellationToken);

        Task<UserAccountManagerResponse> CreateUserMapping(User user, IUnitOfWork unitOfWork, CancellationToken cancellationToken);

        Task<UserAccountManagerResponse> CreateUserGroup(string userGroupName, CancellationToken cancellationToken);

        Task<UserAccountManagerResponse> CreateUserUserGroupAndUserMapping(User user, UserInfo userInfo, IUnitOfWorkFactory unitOfWorkFactory, IPermissionService permissionService, CancellationToken cancellationToken);

        Task<UserAccountManagerResponse> CheckUserGroup(string userGroupName, CancellationToken cancellationToken);

        Task<UserAccountManagerResponse> CheckUser(User user, CancellationToken cancellationToken);

        Task<UserAccountManagerResponse> CheckUserMapper(User user, CancellationToken cancellationToken);

        Task<PasswordResetResult> SetTempPasswordAndVerificationToken(User user, CancellationToken cancellationToken);

        Task ResetUserPassword(int userId, string password, CancellationToken cancellationToken);

        Task UpdateUserPassword(int userId, string password, CancellationToken cancellationToken);

        Task<UserAccountManagerResponse> UpdateUserName(int userId, string userName, CancellationToken cancellationToken);

        Task<UserAccountManagerResponse> UpdateUser(User currentUser, User newUser, IUnitOfWorkFactory unitOfWorkFactory, CancellationToken cancellationToken);        
    }
}
