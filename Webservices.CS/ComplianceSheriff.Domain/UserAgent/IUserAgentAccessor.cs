using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.UserAgent
{
    public interface IUserAgentAccessor
    {
        Task<IEnumerable<UserAgentListItem>> GetUserAgentList(string organizationId, CancellationToken cancellationToken);

        Task<UserAgentListItem> GetUserAgentItem(string userAgentLabel, string organizationId, CancellationToken cancellationToken);
    }
}
