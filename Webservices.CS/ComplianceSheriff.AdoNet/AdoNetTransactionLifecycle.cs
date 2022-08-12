using ComplianceSheriff.Work;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet
{
    class AdoNetTransactionLifecycle : IUnitOfWorkLifecycleManagement
    {
        private readonly IConnectionManager manager;
        private DbTransaction transaction;

        public AdoNetTransactionLifecycle(IConnectionManager manager)
        {
            this.manager = manager;
        }

        public async Task Prepare(CancellationToken cancellationToken)
        {
            var connection = await manager.GetOpenDbConnection(cancellationToken);
            this.transaction = await manager.BeginTransaction(cancellationToken);
        }

        public Task Commit()
        {
            transaction.Commit();
            return Task.CompletedTask;
        }

        public Task Rollback()
        {
            transaction.Rollback();
            return Task.CompletedTask;
        }
    }
}
