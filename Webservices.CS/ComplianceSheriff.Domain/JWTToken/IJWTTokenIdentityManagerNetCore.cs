using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.JWTToken
{
    public interface IJWTTokenIdentityManagerNetCore
    {
        ClaimsIdentity GetIdentity();
        Task Load(IPrincipal user, string organizationId, string orgVirtualDir, CancellationToken cancellationToken);
    }
}
