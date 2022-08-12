using DeKreyConsulting.AdoTestability;
using DeKreyConsulting.AdoTestability.Testing.Moq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet
{
    using CommandSetup = Dictionary<CommandBuilder, SetupCommandBuilderMock>;

    public class ConnectionSetup
    {
        public ConnectionSetup(CommandSetup commandSetup)
        {
            var commandBuilderMocks = CommandBuilderMocks.SetupFor(commandSetup);
            CommandBuilderMocks = commandBuilderMocks;
            ConnectionManager = new Mock<IConnectionManager>();
            Transaction = new Mock<DbTransaction>();
            Transaction.Setup(transaction => transaction.Commit()).Verifiable();
            Transaction.Setup(transaction => transaction.Rollback()).Verifiable();

            ConnectionManager.Setup(mgr => mgr.GetOpenDbConnection(AnyCancellationToken))
                .ReturnsWithDelay(commandBuilderMocks.Connection.Object);
            ConnectionManager.Setup(mgr => mgr.BeginTransaction(AnyCancellationToken))
                .ReturnsWithDelay(Transaction.Object);
            ConnectionManager.Setup(mgr => mgr.Transaction).Returns(Transaction.Object);
        }

        public CommandBuilderMocks CommandBuilderMocks { get; }
        public Mock<DbTransaction> Transaction { get; }
        public Mock<IConnectionManager> ConnectionManager { get; }

        public IServiceCollection AddServices(IServiceCollection services)
        {
            return services
                    .AddScoped<AdoNetTransactionLifecycle>()
                    .AddSingleton(ConnectionManager.Object);
        }

        // TODO - move this to a better spot
        public static CancellationToken AnyCancellationToken =>
            It.IsAny<CancellationToken>();

    }
}
