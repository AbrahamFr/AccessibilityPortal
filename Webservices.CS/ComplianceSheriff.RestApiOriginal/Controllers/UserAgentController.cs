using ComplianceSheriff.Authentication;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.UserAgent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    public class UserAgentController : Controller
    {
        private readonly ILogger<UserAgentController> _logger;
        private readonly JwtSignInHandler _jwtSignInHandler;

        public UserAgentController([FromServices] JwtSignInHandler jwtSignInHandler,
                                   ILogger<UserAgentController> logger)
        {
            _jwtSignInHandler = jwtSignInHandler;
            _logger = logger;
        }

        [HttpGet("userAgentList")]
        public async Task<IActionResult> GetUserAgentList([FromServices] IUserAgentAccessor accessor,
                                                       CancellationToken cancellationToken)
        {
            var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            var result = await accessor.GetUserAgentList(orgId, cancellationToken);

            return Ok(result);
        }

        [HttpGet("getUserAgent/{userAgentLabel}")]
        public async Task<IActionResult> GetUserAgent(string userAgentLabel, [FromServices] IUserAgentAccessor accessor,
                                                       CancellationToken cancellationToken)
        {
            var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            UserAgentListItem userAgentValue = await accessor.GetUserAgentItem(userAgentLabel, orgId, cancellationToken);

            return Ok(userAgentValue);
        }
    }
}
