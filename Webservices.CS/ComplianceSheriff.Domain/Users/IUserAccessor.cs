using ComplianceSheriff.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Users
{
    public interface IUserAccessor
    {
        Task<User> GetUserByUserId(int userId, CancellationToken cancellationToken);
        Task<User> GetUserByUserName(string userName, CancellationToken cancellationToken);

        Task<User> GetUserRecordByUserName(string userName, CancellationToken cancellationToken);
    }
}
