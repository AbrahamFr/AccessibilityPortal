using ComplianceSheriff.Checkpoints;
using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.Scans;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace ComplianceSheriff.CheckpointGroups
{
    public class CheckpointGroupings
    {
        public List<CheckpointGroupScanAssociation> CheckpointGroupScanAssociations { get; set; }

        public List<CheckpointGroup> CheckpointGroups { get; set; }

        public List<CheckpointGroupSubGroup> CheckpointGroupSubGroups { get; set; }

        public List<CheckpointGroupCheckpoint> CheckpointGroupCheckpoints { get; set; }

        public List<Checkpoint> Checkpoints { get; set; }

        public List<ScanGroup> ScanGroups { get; set; }

        public List<ScanByCheckpointGroup> ScansByCheckpointGroup { get; set; }

        public List<ScanGroupScan> ScanGroupScans { get; set; }

        public List<ScanGroupSubGroup> ScanGroupSubGroups { get; set; }

        public HybridDictionary CheckpointGroupDescriptionDictionary { get; set; }

        public CheckpointGroupings()
        {
            CheckpointGroupScanAssociations = new List<CheckpointGroupScanAssociation>();
            CheckpointGroups = new List<CheckpointGroup>();
            CheckpointGroupSubGroups = new List<CheckpointGroupSubGroup>();
            CheckpointGroupCheckpoints = new List<CheckpointGroupCheckpoint>();
            Checkpoints = new List<Checkpoint>();
            ScanGroups = new List<ScanGroup>();
            ScansByCheckpointGroup = new List<ScanByCheckpointGroup>();
            ScanGroupScans = new List<ScanGroupScan>();
            ScanGroupSubGroups = new List<ScanGroupSubGroup>();
            CheckpointGroupDescriptionDictionary = new HybridDictionary();
        }
    }
}
