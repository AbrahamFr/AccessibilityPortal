using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.ScanGroups
{
    public interface IScanGroupService
    {
        Task UpdateScanGroupName(int scanGroupId, string scanGroupName, HttpContext context, CancellationToken cancellationToken);
        Task<int> CreateScanGroup(string scanGroupName, int userGroupId, HttpContext context, bool setAsDefault, CancellationToken cancellationToken);

        Task<ScanGroup> GetScanGroupById(int scanGroupId, CancellationToken cancellationToken);

    }
}
