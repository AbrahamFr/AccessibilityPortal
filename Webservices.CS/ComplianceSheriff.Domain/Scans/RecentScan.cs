using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;

namespace ComplianceSheriff.Scans
{
    public class RecentScan
    {
        public int ScanId { get; set; }
        public int? RunId { get; set; }
        public string ScanName { get; set; }
        public int? HealthScore { get; set; }
        public int? Status { get; set; }
        public string CheckpointGroupDescription { get; set; }

        public string CheckpointGroupId { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public string StartingUrl { get; set; }
        public int TotalPagesRan { get; set; }

        public int PageLimit { get; set; }
        public int Levels { get; set; }
        public int ScannedLevels { get; set; }
        public int? PreviousRunHealthScore { get; set; }
        public int? HealthScoreChange { get; set; }
        public decimal? HealthScoreChangePercent { get; set; }


    }
}
