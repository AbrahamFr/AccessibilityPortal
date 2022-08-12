using ComplianceSheriff.AdoNet.ScanGroups;
using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.Scans;
using ComplianceSheriff.Work;
using DeKreyConsulting.AdoTestability;
using DeKreyConsulting.AdoTestability.Testing.Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.AdoNet.ScanGroups
{
    using CommandSetup = Dictionary<CommandBuilder, SetupCommandBuilderMock>;

    [TestClass]
    public class ScanGroupMutatorShould : ScanGroupMutatorBaseShould<ScanGroupMutatorShould.Setup>
    {
        [TestMethod]
        [TestCategory("Database")]
        public void HaveAValidUpdateScanGroupNameCommand() =>
            ScanGroupMutator.UpdateScanGroupNameCommand.ExplainSingleResult(BuildSqlConnection());

        public class Setup : IScanGroupMutatorSetup
        {
            private readonly ConnectionSetup connectionSetup;

            public Setup(CommandSetup commandSetup)
            {
                this.connectionSetup = new ConnectionSetup(commandSetup);
                Services = connectionSetup.AddServices(new ServiceCollection())
                    .AddComplianceSheriffDomainServices()
                    .BuildServiceProvider();
                Target = new ScanGroupMutator();
            }

            public CommandBuilderMocks CommandBuilderMocks => connectionSetup.CommandBuilderMocks;
            public Mock<DbTransaction> Transaction => connectionSetup.Transaction;
            public Mock<IConnectionManager> ConnectionManager => connectionSetup.ConnectionManager;
            public IServiceProvider Services { get; }
            public IScanGroupMutator Target { get; }
        }

        private SqlConnection BuildSqlConnection()
        {
            return new SqlConnection("Server=devSQL01F001.ad.cryptzone.com;Database=FARM004_CSU001h16;User Id=cs.api;Password=Hisoftware1;Trusted_Connection=false;MultipleActiveResultSets=true");
        }

        protected override Setup GetSetupForUpdateAScanGroupName((int scanGroupId, string name) values)
        {
            return new Setup(new CommandSetup
            {
                { ScanGroupMutator.UpdateScanGroupNameCommand, (mockCmd, record) =>
                    mockCmd.Setup(cmd => cmd.ExecuteNonQueryAsync(AnyCancellationToken))
                        .ReturnsWithDelay(1).Callback(record) }
            });
        }

        protected override void AssertUpdateAScanGroupName(Setup setup, (int scanGroupId, string name) values)
        {
            Assert.AreEqual(1, setup.CommandBuilderMocks.Executions[ScanGroupMutator.UpdateScanGroupNameCommand].Count);
            var execution = setup.CommandBuilderMocks.Executions[ScanGroupMutator.UpdateScanGroupNameCommand].Single();
            Assert.AreEqual("Some Name", execution["@Name"]);
            Assert.AreEqual(1000, execution["@ScanGroupId"]);
        }

        protected override void AssertUpdateAScanGroupNameWasNotRun(Setup setup)
        {
            Assert.IsFalse(setup.CommandBuilderMocks.Executions.ContainsKey(ScanGroupMutator.UpdateScanGroupNameCommand));
        }

        private static CancellationToken AnyCancellationToken =>
            It.IsAny<CancellationToken>();

    }
}
