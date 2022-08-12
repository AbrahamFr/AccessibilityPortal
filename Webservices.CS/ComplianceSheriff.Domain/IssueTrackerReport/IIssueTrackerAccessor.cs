using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.IssueTrackerReport
{
    public interface IIssueTrackerAccessor
    {
        Task<IssueTrackerResponse> GetIssueTrackerResults(int userGroupId, string organizationId, IssueTrackerRequestBase request, CancellationToken cancellationToken);

        Task<OccurrencesResponse> GetOccurrences(int userGroupId, string organizationId, OccurrencesRequest request, CancellationToken cancellationToken);

        Task<OccurrencesByPageResponse> GetOccurrencesByPage(int userGroupId, string organizationId, OccurrencesByPageRequest request, CancellationToken cancellationToken);

        Task<IEnumerable<OccurrencesExportItem>> GetOccurrencesExport(int userGroupId, string organizationId, OccurrencesExportRequest request, CancellationToken cancellationToken);
    }
}
