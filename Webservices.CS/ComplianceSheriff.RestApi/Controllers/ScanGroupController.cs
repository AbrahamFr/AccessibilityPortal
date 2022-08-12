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
using ComplianceSheriff.RestApi.WebResponse;
using ComplianceSheriff.Requests;
using ComplianceSheriff.UserGroups;
using ComplianceSheriff.Permission;
using ComplianceSheriff.Work;
using ComplianceSheriff.ModelState;
using Microsoft.AspNetCore.Http;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    [Authorize]
    public class ScanGroupController : Controller
    {
        private string _currentActionName;
        private string _currentControllerName;

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

        [HttpPut("updateName")]
        [Authorize(Roles = "ScanGroupEditor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateScanGroupName([FromBody] UpdateScanGroupNameRequest request,
                                                         [FromServices] IPermissionAccessor permissionAccessor,
                                                         [FromServices] IScanGroupService scanGroupService,
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

            var scanGroup = await scanGroupService.GetScanGroupById((int)request.ScanGroupId, cancellationToken);

            if (scanGroup == null)
            {
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = $"api:{_currentControllerName}:{_currentActionName}:invalidScanGroup" });
            }


            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());
            var hasEditPermission = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup((int)request.ScanGroupId, userGroupId, typeof(ScanGroup).Name, "edit", cancellationToken);
            if (hasEditPermission)
            {
                await scanGroupService.UpdateScanGroupName((int)request.ScanGroupId, request.ScanGroupDisplayName, HttpContext, cancellationToken);
            } else
            {
                return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = $"api:{_currentControllerName}:{_currentActionName}:noPermissionToUpdateScanGroup" });
            }

            return Ok(new ApiResponse { StatusCode = 200, Data = new Dictionary<string, object> { { "success", true } } });
        }

        [HttpPost("create")]
        [ProducesResponseType(201)]
        [Authorize(Roles = "ScanGroupCreator")]
        public async Task<IActionResult> CreateScanGroup([FromBody] NewScanGroupRequest request,
                                                         [FromServices] IScanGroupService scanGroupService,
                                                         [FromServices] JwtSignInHandler jwtHandler,
                                                         [FromServices] IModelStateService modelStateService,
                                                         CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _currentActionName = Request.Path.Value.Split('/').Last();
                _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }

            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());
            var scanGroupId = await scanGroupService.CreateScanGroup(request.ScanGroupName, userGroupId, HttpContext, Boolean.Parse(request.SetAsDefault), cancellationToken);

            return StatusCode(201, new ApiResponse { StatusCode = 201, Data = new Dictionary<string, object> { { "scanGroupId", scanGroupId } } });
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
