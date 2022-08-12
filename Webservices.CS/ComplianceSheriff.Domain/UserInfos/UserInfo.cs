using System;

namespace ComplianceSheriff.UserInfos
{
    public class UserInfo
    {
        public string UserInfoId { get; set; }
        public int DashboardMode { get; set; } = 0;
        public string DashboardViews { get; set; } = String.Empty;

        public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;  //SET TO LOCAL TIMEZONE OF IIS SERVER

        public int MaxShortLength { get; set; } = 60;

        public int MaxLongLength { get; set; } = 80;

        public int MaxUrlLength { get; set; } = 80;

        public bool UseScriptEditor { get; set; } = false;

        public bool AutoUpdate { get; set; } = true;

        public int? LastScanAccessed { get; set; } = 0;

        public int? CurrentScanGroupFilter { get; set; } = 0;

        public int AutoUpdateInterval { get; set; } = 30;
        public string EmailAddress { get; set; }
        public string StyleSheet { get; set; } = String.Empty;
        public DateTime DateModified { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
