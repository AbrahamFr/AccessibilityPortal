using ComplianceSheriff.Authentication;
using ComplianceSheriff.Checkpoints;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.Reporting;
using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.Scans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    [Authorize(Roles = "TrendReport")]
    public class TrendController : Controller
    {
        private readonly ILogger<TrendController> _logger;

        public TrendController(ILogger<TrendController> logger)
        {
            _logger = logger;
        }

        [HttpPost("scangroupscanperformance")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ScanPerformance>))]
        public async Task<IActionResult> GetScanGroupScanPerformance([FromBody] ScanGroupMetricsRequest request, CancellationToken cancellationToken, [FromServices] IScanGroupScanAccessor accessor, [FromServices] JwtSignInHandler jwtHandler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());

            var result = await accessor.GetScanPerformanceByScanGroup(
                    scanGroupId: request.ScanGroupID,
                    userGroupId: userGroupId,
                    cancellationToken: cancellationToken);

            return Ok(result);
        }

        [HttpPost("scangroupscanhistory")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ScanGroupHistory>))]
        public async Task<IActionResult> GetScanGroupScanHistory([FromBody] ScanGroupMetricsRequest request, CancellationToken cancellationToken, [FromServices] IScanGroupScanAccessor accessor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await accessor.GetScanGroupHistory(
                    scanGroupId: request.ScanGroupID,
                    cancellationToken: cancellationToken);

            return Ok(result);
        }

        [HttpPost("scangrouptop10checkpointfailures")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CheckpointFailure>))]
        public async Task<IActionResult> GetScanGroupTop10CheckpointFailures([FromBody] ScanGroupMetricsRequest request, CancellationToken cancellationToken, [FromServices] ICheckpointAccessor accessor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await accessor.GetTop10CheckpointFailures(
                    scanGroupId: request.ScanGroupID,
                    cancellationToken: cancellationToken);

            return Ok(result);
        }

        [HttpPost("scangrouptop10pagefailures")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PageFailure>))]
        public async Task<IActionResult> GetScanGroupTop10PageFailures([FromBody] ScanGroupMetricsRequest request, CancellationToken cancellationToken, [FromServices] IScanGroupScanAccessor accessor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await accessor.GetTop10PageFailures(
                    scanGroupId: request.ScanGroupID,                    
                    cancellationToken: cancellationToken);

            return Ok(result);
        }

        [HttpPost("scangrouppageperformancemetrics")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ScanGroupPerformanceMetrics>))]
        public async Task<IActionResult> GetScanGroupPagePerformanceMetrics([FromBody] ScanGroupMetricsRequest request, CancellationToken cancellationToken, [FromServices] IScanGroupScanAccessor accessor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await accessor.GetPagePerformanceMetrics(
                    scanGroupId: request.ScanGroupID,
                    cancellationToken: cancellationToken);

            return Ok(result);
        }

        [HttpPost("scangroupcheckpointperformancemetrics")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ScanGroupPerformanceMetrics>))]
        public async Task<IActionResult> GetScanGroupCheckpointPerformanceMetrics([FromBody] ScanGroupMetricsRequest request, CancellationToken cancellationToken, [FromServices] IScanGroupScanAccessor accessor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await accessor.GetCheckPointPerformanceMetrics(
                    scanGroupId: request.ScanGroupID,
                    cancellationToken: cancellationToken);

            return Ok(result);
        }
    }
}
