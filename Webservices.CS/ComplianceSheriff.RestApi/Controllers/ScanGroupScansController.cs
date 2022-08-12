using ComplianceSheriff.Authentication;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.ModelState;
using ComplianceSheriff.Requests;
using ComplianceSheriff.RestApi.WebResponse;
using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.ScanGroupScans;
using ComplianceSheriff.ScanGroupSubGroups;
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
    [Authorize]
    public class ScanGroupScansController : Controller
    {
        private string _currentActionName;
        private string _currentControllerName;

        private readonly ILogger<ScanGroupScansController> _logger;

        public ScanGroupScansController(ILogger<ScanGroupScansController> logger)
        {
            this._logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddScanGroupScans([FromBody] AddScanGroupScansRequest request,
                                            [FromServices] IScanGroupScansAccessor scanGroupScansAccessor,
                                            [FromServices] IScanGroupScansMutator scanGroupScansMutator,
                                            [FromServices] IScanGroupSubGroupAccessor scanGroupSubGroupAccessor,
                                            [FromServices] IScanGroupAccessor scanGroupAccessor,
                                            [FromServices] IScanAccessor scanAccessor,
                                            [FromServices] JwtSignInHandler jwtHandler,
                                            [FromServices] IModelStateService modelStateService,
                                            CancellationToken cancellationToken)
        {

            _currentActionName = Request.Path.Value.Split('/').Last();
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            if (!ModelState.IsValid)
            {
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }

            var scanAddedCounter = 0;
            var scanGroup = await scanGroupAccessor.GetScanGroupByScanGroupId(request.ScanGroupId, cancellationToken);

            if (scanGroup == null)
            {
                _logger.LogError($"ScanGroupId {request.ScanGroupId} does not exist");
                return BadRequest(new ApiResponse { StatusCode = 400, ErrorCode = string.Format("api:{0}:{1}:invalidScanGroupId", _currentControllerName, _currentActionName) });
            }

            //Check if the scanGroup already has a subgroup and then error if that is the case
            var scanGroupSubGroups = await scanGroupSubGroupAccessor.GetScanGroupSubGroupsByScanGroupId(request.ScanGroupId, cancellationToken);

            if (scanGroupSubGroups.Count() > 0)
            {
                _logger.LogCritical($"ScanGroupId {request.ScanGroupId} already contains subgroups so no scans can be added to this scangroup");
                return BadRequest(new ApiResponse { StatusCode = 400, ErrorCode = string.Format("api:{0}:{1}:scanGroupAlreadyContainsScanGroups", _currentControllerName, _currentActionName) });
            }

            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());
            var scanList = await scanAccessor.GetScanIdList(userGroupId, request.ScanList, cancellationToken);

            if (scanList.Count() != request.ScanList.Count())
            {
                _logger.LogCritical($"One or more scans do not exist or are inaccessible to the user, so no scans will be added to Scangroup {scanGroup.ScanGroupId}");
                return BadRequest(new ApiResponse { StatusCode = 400, ErrorCode = string.Format("api:{0}:{1}:oneOrMoreScansAreInvalidOrInaccessible", _currentControllerName, _currentActionName) });
            }

            foreach (int scanId in request.ScanList)
            {               
                var scanGroupScan = await scanGroupScansAccessor.GetScanGroupByScanGroupIdAndScanId(request.ScanGroupId, scanId, cancellationToken);
                if (scanGroupScan == null)
                {
                    await scanGroupScansMutator.AddScanGroupScan(request.ScanGroupId, scanId);
                    scanAddedCounter++;
                } else
                {
                    _logger.LogWarning($"ScanId {scanId} already exists in ScanGroupId {scanGroup.ScanGroupId}");
                }
            }
            
            return Ok(new ApiResponse { StatusCode = 200, Data = new Dictionary<string, object> { { "results", $"{scanAddedCounter} out of {request.ScanList.Count()} scans added to ScanGroup {request.ScanGroupId}" } } });
        }
    }
}
