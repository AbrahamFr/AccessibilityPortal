using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Checkpoints
{
    public interface IIssuesAccessor
    {
        Task<IEnumerable<Issue>> GetTop10Issues(int? scanGroupId, DateRange range, CancellationToken cancellationToken);
    }
}
