using ComplianceSheriff.Work;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet
{
    static class UnitOfWorkExtensions
    {
        public static void PreWorkSql(this IUnitOfWork unitOfWork, Func<IConnectionManager, CancellationToken, Task> deferred)
        {
            unitOfWork.PreWorkSql((context, sp, cancellation) => deferred(context, cancellation));
        }

        public static void PreWorkSql(this IUnitOfWork unitOfWork, Func<IConnectionManager, IServiceProvider, CancellationToken, Task> deferred)
        {
            PrepareUnitOfWork(unitOfWork);
            unitOfWork.Prepare(async (sp, cancellation) =>
            {
                var connection = sp.GetScopedConnection();
                await deferred(connection, sp, cancellation);
            });
        }

        public static void DeferSql(this IUnitOfWork unitOfWork, Func<IConnectionManager, CancellationToken, Task> deferred)
        {
            unitOfWork.DeferSql((context, sp, cancellation) => deferred(context, cancellation));
        }

        public static void DeferSql(this IUnitOfWork unitOfWork, Func<IConnectionManager, IServiceProvider, CancellationToken, Task> deferred)
        {
            PrepareUnitOfWork(unitOfWork);
            unitOfWork.Defer(async (sp, cancellation) =>
            {
                var connection = sp.GetScopedConnection();
                await deferred(connection, sp, cancellation);
            });
        }


        private static void PrepareUnitOfWork(IUnitOfWork unitOfWork)
        {
            unitOfWork.PrepareAndFinalize<AdoNetTransactionLifecycle>();
        }
        private static IConnectionManager GetScopedConnection(this IServiceProvider scope) =>
            scope.GetRequiredService<IConnectionManager>();
    }
}
