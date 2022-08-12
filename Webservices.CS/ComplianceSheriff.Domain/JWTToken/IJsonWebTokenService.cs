using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.JWTToken
{
    public interface IJsonWebTokenService
    {
        Task<string> BuildJwtToken(IPrincipal user, string organizationId, CancellationToken cancellationToken);
    }
}
