using ComplianceSheriff.Authentication;
using ComplianceSheriff.Checkpoints;
using ComplianceSheriff.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    [Authorize]
    public class CheckpointController : Controller
    {
        [HttpGet("checkpointList")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CheckpointCheckpointGroupItem>))]
        public async Task<IActionResult> GetLicensedCheckpointList([FromServices] ICheckpointAccessor accessor,
                                                                   [FromServices] JwtSignInHandler jwtHandler,
                                                                   CancellationToken cancellationToken)
        {
            var organizationId = RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["organizationId"].ToString();

            var result = await accessor.GetLicensedCheckpoints(organizationId, cancellationToken);

            return Ok(result);
        }
    }
}
