using ComplianceSheriff.ScanGroups;
using System.Collections;
using System.Collections.Specialized;

namespace ComplianceSheriff.CheckpointGroups
{
    public interface ICheckpointGroupService
    {
        void GetCheckpointIds(CheckpointGroup group, StringCollection checkIds, CheckpointGroupings checkpointGroupings);
        string[] GetPermittedCheckpointGroups(StringCollection masterIds, CheckpointGroupings checkpointGroupings);

        string[] GetPermittedCheckpointGroups(ScanGroup scanGroup, CheckpointGroupings checkpointGroupings);

        string[] GetPermittedCheckpointGroupsByName(StringCollection masterIds, CheckpointGroupings checkpointGroupings);

        string MakeScriptString(string text);
        string[] ToArray(StringCollection collection);
    }
}