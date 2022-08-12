using ComplianceSheriff.Passwords;
using ComplianceSheriff.Work;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ComplianceSheriff.Users
{
    public interface IUserMutator
    {
        Task<int> AddUser(User user);

        Task RemoveUser(User user);

        Task UpdateUserName(string userName, int userId);

        void UpdateUser(User user, IUnitOfWork unitOfWork);

        void AddUserMapper(User user, IUnitOfWork unitOfWork);

        Task RemoveUserMapper(User user);

        Task SetTempPasswordAndVerificationToken(User user, string tempPassword, HashResult hashResult);

        Task UnsetTempPassword(User user);
        
        Task ResetUserPassword(int userId, string password);

        Task UpdateUserPassword(int userId, string password);
    }
}
