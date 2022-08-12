using ComplianceSheriff.Work;
using System.Threading.Tasks;

namespace ComplianceSheriff.ScanGroups
{
    public interface IScanGroupMutator
    {
        void UpdateScanGroupName(int scanGroupId, string name, IUnitOfWork unitOfWork);
        Task<int> AddScanGroup(string scanGroupName);
    }
}
