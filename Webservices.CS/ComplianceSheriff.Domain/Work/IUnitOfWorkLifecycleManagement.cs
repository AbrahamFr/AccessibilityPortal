using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Work
{
    public interface IUnitOfWorkLifecycleManagement
    {
        Task Prepare(CancellationToken cancellationToken);
        Task Commit();
        Task Rollback();
    }
}
