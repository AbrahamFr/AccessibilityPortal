using ComplianceSheriff.Authentication;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.ModelState;
using ComplianceSheriff.Requests;
using ComplianceSheriff.RestApi.WebResponse;
using ComplianceSheriff.UserGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    public class UserGroupController : Controller
    {
        private string _currentActionName;
        private string _currentControllerName;

        private readonly ILogger<UserGroupController> _logger;

        public UserGroupController(ILogger<UserGroupController> logger)
        {
            this._logger = logger;
        }

        [Authorize]
        [HttpPut("updateName")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUserGroupName([FromBody] UpdateUserGroupNameRequest request,
                                                             [FromServices] IUserGroupsService userGroupsService,
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

            var userGroup = await userGroupsService.GetUserGroupById((int)request.UserGroupId, cancellationToken);

            if (userGroup == null)
            {
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = $"api:{_currentControllerName}:{_currentActionName}:invalidUserGroup" });
            }

            var jwtUserGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());
            var jwtUserName = RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userName"].ToString();

            if (jwtUserGroupId != request.UserGroupId)
            {
                _logger.LogWarning($"User {jwtUserName}, a member of UserGroupId {jwtUserGroupId} attempted to update UserGroupId {request.UserGroupId} with Name {request.UserGroupName}");
                return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = $"api:{_currentControllerName}:{_currentActionName}:noPermissionToUpdateUserGroup" });
            }

            await userGroupsService.UpdateUserGroupName((int)request.UserGroupId, request.UserGroupName, cancellationToken);

            return Ok(new ApiResponse { StatusCode = 200, Data = new Dictionary<string, object> { { "success", true } } });
        }
    }
}
