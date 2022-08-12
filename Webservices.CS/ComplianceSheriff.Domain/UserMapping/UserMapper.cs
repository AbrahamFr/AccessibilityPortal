using System;

namespace ComplianceSheriff.UserMapping
{
    public class UserMapper
    {
        public int UserMapperId { get; set; }
        public string OrganizationId { get; set; }
        public int? OrgUserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
