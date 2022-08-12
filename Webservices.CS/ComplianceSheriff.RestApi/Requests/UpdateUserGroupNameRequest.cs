using System.ComponentModel.DataAnnotations;

namespace ComplianceSheriff.Requests
{
    public class UpdateUserGroupNameRequest
    {
        [Required(ErrorMessage = "{0}IsRequired")]
        public int? UserGroupId { get; set; }

        [Required(ErrorMessage = "{0}IsRequired")]
        public string UserGroupName { get; set; }
    }
}
