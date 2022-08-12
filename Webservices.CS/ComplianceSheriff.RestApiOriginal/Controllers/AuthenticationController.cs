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

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    public class AuthenticationController : Controller
    {
        private string _currentActionName;
        private string _currentControllerName;

        [HttpPost("authenticate")]
        public async Task<IActionResult> GetJwtToken([FromForm] AuthenticationRequest request, 
                                                     [FromServices] IAuthenticationService authService,
                                                     [FromServices] IModelStateService modelStateService,
                                                     [FromServices] IAuthAccessor authAccessor,
                                                     [FromServices] IJsonWebTokenService jsonWebTokenService,
                                                     CancellationToken cancellationToken)
        {
            var jwtToken = string.Empty;
            _currentActionName = Request.Path.Value.Split('/').Last();
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            if (!ModelState.IsValid)
            {
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }

            var authenticationType = System.Enum.TryParse(request.AuthenticationType, out AuthenticationTypes result) == true ? result : AuthenticationTypes.FormsAuthentication;
            var user = await authService.AuthenticateUser(request.UserName, request.Password.Replace("\"",""), authenticationType, cancellationToken);
          
            if (user == null)
            {
                return Unauthorized($"api:{_currentControllerName}:{_currentActionName}:userNotAuthenticated");
            }

            if (user != null)
            {
                jwtToken = await jsonWebTokenService.BuildJwtToken((IPrincipal)user, request.OrganizationId, cancellationToken);
                return Ok(jwtToken);
            }
           
            return null;
        }
    }
}
