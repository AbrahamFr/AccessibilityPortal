using ComplianceSheriff.ApiRoles;
using ComplianceSheriff.UserGroups;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.JWTToken
{
    public class JWTTokentIdentityManagerNetCore : IJWTTokenIdentityManagerNetCore
    {
        private IPrincipal _user;
        private string _userName;
        private string _organizationId;
        private string _orgVirtualDir;
        protected ClaimsIdentity _identity;
        private UserGroup _userGroup;
        private readonly IUserGroupAccessor _userGroupAccessor;
        private readonly IApiRoleAccessor _apiRoleAccessor;

        public JWTTokentIdentityManagerNetCore(IUserGroupAccessor userGroupAccessor,
                                               IApiRoleAccessor apiRoleAccessor)
        { 
            _userGroupAccessor = userGroupAccessor;
            _apiRoleAccessor = apiRoleAccessor;
        }

        public async Task Load(IPrincipal user, string organizationId, string orgVirtualDir, CancellationToken cancellationToken)
        {
            _user = user;
            _userName = user.Identity.Name;
            _organizationId = organizationId;
            _orgVirtualDir = orgVirtualDir;

            await BuildIdentity(cancellationToken);
        }

        public ClaimsIdentity GetIdentity()
        {            
            return _identity;
        }

        private async Task BuildIdentity(CancellationToken cancellationToken)
        {
            _identity = new ClaimsIdentity(new GenericIdentity(_userName, "TokenAuth"));
            _userGroup = await GetUserGroup(cancellationToken);

            AddClaim("organizationId", _organizationId.ToString());
            AddClaim("userName", _userName);
            AddClaim("orgVirtualDir", _orgVirtualDir);
            AddClaim("userGroupId", _userGroup.UserGroupId.ToString());
            AddClaim("scanGroupId", _userGroup.ScanGroupId.ToString());

            await AddRoles(cancellationToken);
        }

        private void AddClaim(string claimId, string claimValue)
        {
            _identity.AddClaim(new Claim(claimId, claimValue));
        }

        private async Task<UserGroup> GetUserGroup(CancellationToken cancellationToken)
        {
            var userGroup = await _userGroupAccessor.GetUserGroupByUserName(_userName, cancellationToken);
            return userGroup;
        }

        private async Task<IEnumerable<ApiRole>> GetApiRoles(string userGroupName, CancellationToken cancellationToken)
        {
            var userGroupApiRoles = await _apiRoleAccessor.GetApiRolesByUserGroupName(userGroupName, cancellationToken);
            return userGroupApiRoles;
        }

        private async Task AddRoles(CancellationToken cancellationToken)
        {
            var roleClaims = new List<Claim>
            {
                new Claim("roles", "User"),
                new Claim("roles", "ComplianceSheriff")
            };

            //Add Administrator Role
            if (_userGroup.Name.ToUpper().Trim() == "ADMINISTRATORS")
            {
                roleClaims.Add(new Claim("roles", "Administrator"));
            }

            foreach (var role in await GetApiRoles(_userGroup.Name, cancellationToken))
            {
                roleClaims.Add(new Claim("roles", role.JwtRoleName));
            }

            _identity.AddClaims(roleClaims);
        }
    }
}
