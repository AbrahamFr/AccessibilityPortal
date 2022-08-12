using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Work
{
    public interface IUnitOfWork : IDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken);

        void Prepare(Func<IServiceProvider, CancellationToken, Task> action);

        void Defer(Func<IServiceProvider, Task> action);
        void Defer(Func<IServiceProvider, CancellationToken, Task> action);
        void AfterCommit(Func<IServiceProvider, Task> action);

        void PrepareAndFinalize<T>()
            where T : IUnitOfWorkLifecycleManagement;

    }
}
