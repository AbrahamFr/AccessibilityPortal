namespace ComplianceSheriff.Scans
{
    public class ScanPerformance
    {
        public int ScanId { get; set; }
        public string ScanName { get; set; }
        public int ScannedPages { get; set; }
        public int ScannedCheckpoints { get; set; }
        public int CheckpointSuccess { get; set; }

        public int CheckpointFailure { get; set; }

        public double CheckpointFailurePercent { get; set; }
    }
}
