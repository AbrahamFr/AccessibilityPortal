using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserInfos
{
    public interface IUserInfoAccessor
    {
        Task<UserInfo> GetUserInfoByUserInfoId(string userInfoId, CancellationToken cancellationToken);
    }
}
