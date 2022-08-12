using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ComplianceSheriff.Authentication;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.Scans;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    [Authorize]
    public class ScanGroupController : Controller
    {
        private readonly ILogger<ScanGroupController> _logger;

        public ScanGroupController(ILogger<ScanGroupController> logger)
        {
            _logger = logger;
        }

        [HttpGet("scheduledScanGroups")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ScheduledScanGroup>))]
        public async Task<IActionResult> GetScheduledScanGroups(CancellationToken cancellationToken, [FromServices] IScanGroupAccessor accessor, [FromServices] IAuthAccessor authAccessor, [FromServices] JwtSignInHandler jwtHandler, bool scheduledScan = true)
        {
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());

            var result = await accessor.GetScheduledScanGroups(userGroupId, cancellationToken, scheduledScan);
            return Ok(result);
        }

        [HttpGet("allScanGroups")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ScanGroupListItem>))]
        public async Task<IActionResult> GetAllScanGroupsByPermission(CancellationToken cancellationToken, [FromServices] IScanGroupAccessor accessor, [FromServices] IAuthAccessor authAccessor, [FromServices] JwtSignInHandler jwtHandler)
        {
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());

            var result = await accessor.GetAllScanGroupListByPermission(userGroupId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("subGroupsScansByScanGroupId/{scanGroupId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<SubGroupScansResponse>))]
        public async Task<IActionResult> GetSubGroupsAndScansByScanGroupId(int scanGroupId, CancellationToken cancellationToken, [FromServices] IScanGroupAccessor accessor, [FromServices] IAuthAccessor authAccessor, [FromServices] JwtSignInHandler jwtHandler)
        {
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());

            var result = await accessor.GetSubGroupsAndScansByScanGroupId(userGroupId, scanGroupId, cancellationToken);

            return Ok(result);            
        }
    }

    public class ScanGroupReportRequest
    {
        [Required]
        public int? ScanGroupID { get; set; }

        [Required]
        public DateTimeOffset? StartDate { get; set; }

        [Required]
        public DateTimeOffset? EndDate { get; set; }
    }
}
