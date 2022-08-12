namespace ComplianceSheriff.Scans
{
    public class TaskRequest
    {
        public int RunId { get; set; }
        public string ScanId { get; set; }
        public string OrgId { get; set; }

        public string UserName { get; set; }
    }
}