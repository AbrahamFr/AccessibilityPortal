namespace ComplianceSheriff.Scans
{
    public class StartPage
    {
        public string Path { get; set; }
        public string Levels { get; set; }
        public string Script { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string PageLimit { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }

        public override bool Equals(object obj)
        {
            var isStartPage = obj is StartPage;
            if (!isStartPage) return false;

            var compareStartPage = (StartPage)obj;

            return Path == compareStartPage.Path &&
                   Levels == compareStartPage.Levels &&
                   Script == compareStartPage.Script &&
                   Username == compareStartPage.Username &&
                   Password == compareStartPage.Password &&
                   Domain == compareStartPage.Domain &&
                   PageLimit == compareStartPage.PageLimit &&
                   WindowWidth == compareStartPage.WindowWidth &&
                   WindowHeight == compareStartPage.WindowHeight;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
