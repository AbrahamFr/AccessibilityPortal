using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.Work;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.ScanGroups
{
    public interface IScanGroupMutatorSetup
    {
        IScanGroupMutator Target { get; }
        IServiceProvider Services { get; }
    }

    public abstract class ScanGroupMutatorBaseShould<T>
        where T : IScanGroupMutatorSetup
    {
        [TestMethod]
        public async Task BeAbleToUpdateAScanGroupName()
        {
            var values = (scanGroupId: 1000, name: "Some Name");
            var setup = GetSetupForUpdateAScanGroupName(values);

            using (var work = setup.Services.GetRequiredService<IUnitOfWorkFactory>().CreateUnitOfWork())
            {
                setup.Target.UpdateScanGroupName(values.scanGroupId, values.name, work);
                await work.CommitAsync(CancellationToken.None);
            }

            AssertUpdateAScanGroupName(setup, values);
        }

        [TestMethod]
        public void BeAbleToCancelUpdatingAScanGroupName()
        {
            var values = (scanGroupId: 2000, name: "Other Name");
            var setup = GetSetupForUpdateAScanGroupName(values);

            using (var work = setup.Services.GetRequiredService<IUnitOfWorkFactory>().CreateUnitOfWork())
            {
                setup.Target.UpdateScanGroupName(values.scanGroupId, values.name, work);
            }

            AssertUpdateAScanGroupNameWasNotRun(setup);
        }

        protected abstract T GetSetupForUpdateAScanGroupName((int scanGroupId, string name) values);
        protected abstract void AssertUpdateAScanGroupName(T setup, (int scanGroupId, string name) values);
        protected abstract void AssertUpdateAScanGroupNameWasNotRun(T setup);

    }
}
