using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Work
{
    internal class UnitOfWork : IUnitOfWork
    {
        struct PortionOfWork
        {
            public Func<IServiceProvider, CancellationToken, Task> Action;
        }

        private readonly List<PortionOfWork> prework = new List<PortionOfWork>();
        private readonly List<PortionOfWork> work = new List<PortionOfWork>();
        private readonly HashSet<Type> lifecycleManagementTypes = new HashSet<Type>();
        private readonly IServiceProvider provider;
        private readonly List<Func<IServiceProvider, Task>> afterCommit = new List<Func<IServiceProvider, Task>>();

        public UnitOfWork(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public void Prepare(Func<IServiceProvider, CancellationToken, Task> action) =>
            prework.Add(new PortionOfWork
            {
                Action = action,
            });

        public void Defer(Func<IServiceProvider, Task> action) =>
            work.Add(new PortionOfWork
            {
                Action = (sp, _) => action(sp),
            });

        public void Defer(Func<IServiceProvider, CancellationToken, Task> action) =>
            work.Add(new PortionOfWork
            {
                Action = action,
            });

        public void AfterCommit(Func<IServiceProvider, Task> action) =>
            afterCommit.Add(action);

        public void PrepareAndFinalize<T>()
            where T : IUnitOfWorkLifecycleManagement
        {
            lifecycleManagementTypes.Add(typeof(T));
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            using (var scope = provider.CreateScope())
            {
                var values = lifecycleManagementTypes.Select(type => scope.ServiceProvider.GetRequiredService(type) as IUnitOfWorkLifecycleManagement).ToArray();
                try
                {
                    foreach (var portionOfWork in prework)
                    {
                        await portionOfWork.Action(scope.ServiceProvider, cancellationToken);
                    }

                    foreach (var portionOfWork in values)
                    {
                        await portionOfWork.Prepare(cancellationToken);
                    }

                    foreach (var portionOfWork in work)
                    {
                        await portionOfWork.Action(scope.ServiceProvider, cancellationToken);
                    }

                    foreach (var portionOfWork in values)
                    {
                        await portionOfWork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    foreach (var portionOfWork in values)
                    {
                        await portionOfWork.Rollback();
                    }
                    throw;
                }

                foreach (var entry in afterCommit)
                {
                    await entry(scope.ServiceProvider);
                }
            }
        }

        void IDisposable.Dispose()
        {
        }
    }
}
