using ComplianceSheriff.Authentication;
using ComplianceSheriff.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ComplianceSheriff.ModelState;
using ComplianceSheriff.RestApi.WebResponse;
using ComplianceSheriff.JWTToken;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using ComplianceSheriff.Users;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    public class AuthenticationController : Controller
    {
        private string _currentActionName;
        private string _currentControllerName;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
        }

        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetJwtToken([FromBody] AuthenticationRequest request, 
                                                     [FromServices] IAuthenticationService authService,
                                                     [FromServices] IModelStateService modelStateService,
                                                     [FromServices] IAuthAccessor authAccessor,
                                                     [FromServices] IJsonWebTokenService jsonWebTokenService,
                                                     CancellationToken cancellationToken)
        {
            _currentActionName = Request.Path.Value.Split('/').Last();
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            if (!ModelState.IsValid)
            {
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }

            var authenticationType = System.Enum.TryParse(request.AuthenticationType, out AuthenticationTypes result) == true ? result : AuthenticationTypes.FormsAuthentication;
            var authInfo = await authService.AuthenticateUser(request.UserName, request.Password.Replace("\"",""), authenticationType, cancellationToken);

            if (authInfo.IsAuthenticated)
            {               
                string jwtToken = await jsonWebTokenService.BuildJwtToken((IPrincipal)authInfo.User, request.OrganizationId, cancellationToken);
                return Ok(new ApiResponse {
                             Data = new System.Collections.Generic.Dictionary<string, object>{{ "authToken", jwtToken }},
                             StatusCode = 200
                     });
            } else
            {
                return Unauthorized(
                    new ApiResponse{
                        ErrorCode = $"api:{_currentControllerName}:{_currentActionName}:userNotAuthenticated",
                        StatusCode = 401
                    });
            }          
        }


        [Authorize]
        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshJwtToken([FromServices] IAuthenticationService authService,
                                                         [FromServices] IModelStateService modelStateService,
                                                         [FromServices] IAuthAccessor authAccessor,
                                                         [FromServices] IUserAccessor userAccessor,
                                                         [FromServices] JwtSignInHandler jwtSignInHandler,
                                                         [FromServices] IJsonWebTokenService jsonWebTokenService,
                                                         CancellationToken cancellationToken)
        {
            _currentActionName = Request.Path.Value.Split('/').Last();
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            
            var headers = this.Request.Headers;

            if (headers.ContainsKey("Authorization"))
            {
                var tryGetValue = headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues incomingJwtToken);

                if (tryGetValue)
                {
                    var tokenToRead = incomingJwtToken.ToString().Split(' ')[1];
                    var jwtSecurityToken = jwtSignInHandler.ReadJwt(tokenToRead);

                    var payload = jwtSecurityToken.Payload;

                    var user = await userAccessor.GetUserRecordByUserName(payload["userName"].ToString(), cancellationToken);

                    if (user != null)
                    {
                        string authToken = await jsonWebTokenService.BuildJwtToken((IPrincipal)user, payload["organizationId"].ToString(), cancellationToken);
                        return Ok(new ApiResponse { StatusCode = 200, Data = new System.Collections.Generic.Dictionary<string, object> { { "authToken", authToken } } });
                    } else
                    {
                        this._logger.LogError("User Does not Exist for token: " + incomingJwtToken);
                    }                    
                } else
                {
                    this._logger.LogError("JwtToken was unable to be parsed from Header");
                }                                
            }
            else
            {
                this._logger.LogError("Header did not contain Authorization Key: ");
            }

            return Unauthorized(
                 new ApiResponse
                 {
                     ErrorCode = $"api:{_currentControllerName}:{_currentActionName}:userNotAuthenticated",
                     StatusCode = 401
                 });
        }
    }
}
