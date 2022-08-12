using ComplianceSheriff.ScanGroups;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ComplianceSheriff.CheckpointGroups
{
    public class CheckpointGroupService : ICheckpointGroupService
    {
        public string[] GetPermittedCheckpointGroups(ScanGroup scanGroup, CheckpointGroupings checkpointGroupings)
        {
            var result = new StringCollection();
            foreach (var id in scanGroup.Scans)
            {
                var collection = new StringCollection();
                collection.AddRange(checkpointGroupings.ScansByCheckpointGroup.Where(s => s.ScanId == id).Select(c => c.CheckpointGroupId).ToArray());
                result.AddRange(GetPermittedCheckpointGroups(collection,checkpointGroupings));
            }

            foreach (var id in scanGroup.Subgroups)
            {
                var scanGroupSubGroups = checkpointGroupings.ScanGroups.Where(sg => sg.ScanGroupId.ToString() == id);
                if (scanGroupSubGroups.Any())
                {
                    var permittedCheckpointGroups = GetPermittedCheckpointGroups(scanGroupSubGroups.FirstOrDefault(), checkpointGroupings);
                    result.AddRange(permittedCheckpointGroups);
                }
            }
 
            return ToArray(result);
        }

        public string[] GetPermittedCheckpointGroups(StringCollection masterIds, CheckpointGroupings checkpointGroupings)
        {
            var result = new StringCollection();
            var idSet = new StringCollection();
            foreach (var id in masterIds)
            {
                var permittedCheckpointGroup = checkpointGroupings.CheckpointGroups.Where(g => g.CheckpointGroupId == id);
                if(permittedCheckpointGroup.Any())
                {
                    GetCheckpointIds(permittedCheckpointGroup.FirstOrDefault(), idSet, checkpointGroupings);
                }
                
                Add(result, GetCheckpointGroupsContainingAll(idSet, checkpointGroupings));

            }

            return ToArray(result);
        }

        public string[] GetPermittedCheckpointGroupsByName(StringCollection chkGrpNames, CheckpointGroupings checkpointGroupings)
        {
            var result = new StringCollection();
            var idSet = new StringCollection();
            foreach (var name in chkGrpNames)
            {
                var permittedCheckpointGroup = checkpointGroupings.CheckpointGroups.Where(g => g.ShortDescription == name);
                if (permittedCheckpointGroup.Any())
                {
                    GetCheckpointIds(permittedCheckpointGroup.FirstOrDefault(), idSet, checkpointGroupings);
                }

                Add(result, GetCheckpointGroupsContainingAll(idSet, checkpointGroupings));

            }

            return ToArray(result);
        }

        public string[] GetCheckpointGroupsContainingAll(StringCollection idSet, CheckpointGroupings checkpointGroupings)
        {
            var result = new StringCollection();
            foreach (var id in checkpointGroupings.CheckpointGroups.Select(g => g.CheckpointGroupId).ToArray())
            {
                var checkpointGroups = checkpointGroupings.CheckpointGroups.Where(g => g.CheckpointGroupId == id);
                if (checkpointGroups.Any())
                {
                    var group = checkpointGroups.FirstOrDefault();
                    if (!checkpointGroupings.CheckpointGroupDescriptionDictionary.Contains(id))
                        checkpointGroupings.CheckpointGroupDescriptionDictionary[id] = group.ShortDescription;
                    if (AreAllContainedCheckpointsInSet(group, idSet, checkpointGroupings))
                        result.Add(id);
                }
            }

            return ToArray(result);
        }

        public bool AreAllContainedCheckpointsInSet(CheckpointGroup group, StringCollection idSet, CheckpointGroupings checkpointGroupings)
        {
            if (group.Checkpoints.Count == 0 && group.Subgroups.Count == 0)
                return false;
            foreach (var id in group.Checkpoints)
                if (!idSet.Contains(id))
                    return false;
            foreach (var id in group.Subgroups)
            {
                var checkpointGroup = checkpointGroupings.CheckpointGroups.Where(g => g.CheckpointGroupId == id).FirstOrDefault();
                if (!AreAllContainedCheckpointsInSet(checkpointGroup, idSet, checkpointGroupings))
                    return false;
            }

            return true;
        }

        public void Add(StringCollection to, ICollection from)
        {
            foreach (string value in from)
                if (!to.Contains(value))
                    to.Add(value);
        }

        public void GetCheckpointIds(CheckpointGroup group, StringCollection checkIds, CheckpointGroupings checkpointGroupings)
        {
            foreach (String subgroup in group.Subgroups)
            {
                var checkpointGroup = checkpointGroupings.CheckpointGroups.Where(g => g.CheckpointGroupId == subgroup).FirstOrDefault();
                GetCheckpointIds(checkpointGroup, checkIds, checkpointGroupings);
            }

            foreach (string id in group.Checkpoints)
                if (!checkIds.Contains(id))
                    checkIds.Add(id);
        }

        public string MakeScriptString(string text)
        {
            return "'" + text.Replace(@"\", @"\\").Replace("'", @"\'").Replace("\n", @"\n").Replace("\r", @"\r") + "'";
        }

        public string[] ToArray(StringCollection collection)
        {
            string[] result = new string[collection.Count];
            collection.CopyTo(result, 0);
            return result;
        }
    }
}
