using System;

namespace ComplianceSheriff.ScanGroups
{
    public class ScanGroupReport
    {

        public string ReportType { get; set; }
        public int ReportTargetId { get; set; }
        public int? ParentScanGroupId { get; set; }
        public string GroupName { get; set; }
        public string ScanName { get; set; }
        public DateTimeOffset MinRunFinished { get; set; }
        public DateTimeOffset MaxRunFinished { get; set; }
        public ulong TotalAnalyzedOccurrences { get; set; }

        public ulong FailedOccurrences { get; set; }
        public uint Failed_P1_Occurrences { get; set; }
        public uint Failed_P2_Occurrences { get; set; }
        public uint Failed_P3_Occurrences { get; set; }

        public ulong WarningOccurrences { get; set; }

        public uint Warnings_P1_Occurrences { get; set; }
        public uint Warnings_P2_Occurrences { get; set; }
        public uint Warnings_P3_Occurrences { get; set; }

        public uint Failed_P1_or_P2_Occurrences { get; set; }
        public uint Failed_or_Warning_Occurrences { get; set; }
        public uint Failed_or_Warning_P1_Occurrences { get; set; }
        public uint Failed_or_Warning_P1_or_P2_Occurrences { get; set; }
        public uint Occurrences_No_Failures_Or_Warnings { get; set; }
        public uint Passing_P1_Occurrences { get; set; }
        public uint Passing_P1_or_P2_Occurrences { get; set; }

        public uint All_Passing_Occurrences { get; set; }

        public uint Occurrences_needing_Visual_Inspection { get; set; }
        public uint Visual_Inspection_P1_Occurrences { get; set; }
        public uint Visual_Inspection_P2_Occurrences { get; set; }
        public uint Visual_Inspection_P3_Occurrences { get; set; }
        public uint Occurrences_needing_Visual_Inspection_on_P1s_or_P2s { get; set; }

        //public uint Occurrences_needing_Visual_Inspection_on_P1s { get; set; }
        
        public uint Checkpoints { get; set; }
        public uint P1_Checkpoints { get; set; }
        public uint P2_Checkpoints { get; set; }
        public uint P3_Checkpoints { get; set; }
        public uint FailedCheckPoints { get; set; }
        public uint Failed_P1_Checkpoints { get; set; }
        public uint Failed_P2_Checkpoints { get; set; }
        public uint Failed_P3_Checkpoints { get; set; }
        public uint Failed_P1_or_P2_Checkpoints { get; set; }


        public uint Warning_CheckPoints { get; set; }
        public uint Warning_P1_CheckPoints { get; set; }
        public uint Warning_P2_CheckPoints { get; set; }
        public uint Warning_P3_CheckPoints { get; set; }

        public uint Failed_or_Warning_Checkpoints { get; set; }
        public uint Failed_or_Warning_P1_Checkpoints { get; set; }
        public uint Failed_or_Warning_P1_or_P2_Checkpoints { get; set; }

        public uint CheckPoints_needing_Visual_Inspection { get; set; }
        public uint Visual_Inspection_P1_CheckPoints { get; set; }
        public uint Visual_Inspection_P2_CheckPoints { get; set; }
        public uint Visual_Inspection_P3_CheckPoints { get; set; }

        public uint Pages { get; set; }

        public uint Pages_with_Failures { get; set; }
        public uint Pages_P1_Failures { get; set; }
        public uint Pages_P2_Failures { get; set; }
        public uint Pages_P3_Failures { get; set; }

        public uint Pages_with_Warnings { get; set; }
        public uint Pages_P1_Warnings { get; set; }
        public uint Pages_P2_Warnings { get; set; }
        public uint Pages_P3_Warnings { get; set; }
        //public uint Pages_P1_or_P2_Failures { get; set; }
        
        public uint Pages_needing_Visual_Inspection { get; set; }
        public uint Pages_with_P1_Visual_Inspections { get; set; }
        public uint Pages_with_P2_Visual_Inspections { get; set; }
        public uint Pages_with_P3_Visual_Inspections { get; set; }

        public uint Pages_Failures_or_Warnings { get; set; }

        public uint Pages_P1_Failures_or_Warnings { get; set; }
        public uint Pages_P1_or_P2_Failures_or_Warnings { get; set; }

        //public uint Pages_needing_Visual_Inspection_on_P1s { get; set; }
        public uint Pages_needing_Visual_Inspection_on_P1s_or_P2s { get; set; }
    }
}