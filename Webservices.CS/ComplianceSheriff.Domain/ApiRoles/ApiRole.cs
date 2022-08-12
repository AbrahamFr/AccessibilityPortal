using System.Xml.Serialization;

namespace ComplianceSheriff.ApiRoles
{
    public class ApiRole
    {
        [XmlAttribute] public int JwtRoleId;
        [XmlAttribute] public string JwtRoleName;

        public ApiRole()
        {
        }
        public ApiRole(int id, string roleName)
        {
            JwtRoleId = id;
            JwtRoleName = roleName;
        }
    }
}
