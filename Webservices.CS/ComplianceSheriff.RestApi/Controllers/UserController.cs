using ComplianceSheriff.Authentication;
using ComplianceSheriff.Email;
using ComplianceSheriff.Enums;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.ModelState;
using ComplianceSheriff.Passwords;
using ComplianceSheriff.Permission;
using ComplianceSheriff.Requests;
using ComplianceSheriff.Responses;
using ComplianceSheriff.RestApi.WebResponse;
using ComplianceSheriff.TimeZones;
using ComplianceSheriff.Urls;
using ComplianceSheriff.UserAccounts;
using ComplianceSheriff.UserInfos;
using ComplianceSheriff.Users;
using ComplianceSheriff.WebResponse;
using ComplianceSheriff.Work;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private string _responseCode;
        private string _currentActionName;
        private string _currentControllerName;

        private readonly ILogger<UserController> _logger;
        private readonly JwtSignInHandler _jwtSignInHandler;
        private readonly IPermissionService _permissionService;
        private readonly IModelStateService _modelStateService;
        private readonly IUserAccountManagerService _userAccountManagerService;

        public UserController(ILogger<UserController> logger,
                              [FromServices] JwtSignInHandler jwtSignInHandler,
                              [FromServices] IModelStateService modelStateService,
                              [FromServices] IPermissionService permissionService,
                              [FromServices] IUserAccountManagerService userAccountManagerService)
        {
            _logger = logger;
            _jwtSignInHandler = jwtSignInHandler;
            _modelStateService = modelStateService;
            _permissionService = permissionService;
            _userAccountManagerService = userAccountManagerService;
        }

        [Authorize]
        [HttpGet("getUserByUserName/{userName}")]
        [ProducesResponseType(typeof(GenericApiResponse<GetUserByUserNameResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserByUserName(string userName,
                                                [FromServices] IWebResponseService webResponseService,
                                                CancellationToken cancellationToken)
        {

            _currentActionName = Request.Path.Value.Split('/')[3];
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString().ToLower();

            if (!ModelState.IsValid)
            {
                _responseCode = _modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            var user = await _userAccountManagerService.GetUserByUserName(userName, cancellationToken);

            if (user == null)
            {
                _responseCode = String.Format("api:{0}:{1}:userNotFound", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            //Return Success Response            
            var apiResponse = new GenericApiResponse<GetUserByUserNameResponse>
            {
                StatusCode = 200,
                Data = new GetUserByUserNameResponse(user)
            };

            return Ok(apiResponse);
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(GenericApiResponse<UserCreateResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateNewUser([FromBody] CreateUserRequest request,
                                                [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                                [FromServices] IPermissionService permissionService,
                                                [FromServices] IWebResponseService webResponseService,
                                                CancellationToken cancellationToken)
        {
            _currentActionName = Request.Path.Value.Split('/').Last();
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            if (!ModelState.IsValid)
            {
                _responseCode = _modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            var newUser = new User(request.UserName, request.Password, request.FirstName, request.LastName, request.EmailAddress, request.OrganizationId, request.UserGroupName) { };
            var newUserInfo = new UserInfo
            {
                UserInfoId = request.UserName,
                DashboardViews = String.Empty,
                EmailAddress = request.EmailAddress,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now
            };

            UserAccountManagerResponse userAccountManagerResponse = await _userAccountManagerService.CreateUserUserGroupAndUserMapping(newUser, newUserInfo, unitOfWorkFactory, permissionService, cancellationToken);
            var response = webResponseService.CreateUserAccountManagerResponse(userAccountManagerResponse,
                                                                               _currentControllerName,
                                                                               _currentActionName);

            //Return Error Response
            if (response != null)
            {
                if (response.StatusCode == 409)
                {
                    return Conflict(response);
                }
            }

            //Return Success Response
            var apiResponse = new GenericApiResponse<UserCreateResponse>
            {
                StatusCode = 201,
                Data = new UserCreateResponse
                {
                    UserName = request.UserName,
                    OrganizationId = request.OrganizationId
                }
            };
            return Created(HttpContext.Request.Path, apiResponse);
        }

        [Authorize]
        [HttpPut("update")]
        [ProducesResponseType(typeof(GenericApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request,
                                                [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                                [FromServices] IUserAccessor userAccessor,
                                                [FromServices] JwtSignInHandler jwtHandler,
                                                [FromServices] ITimeZoneService timeZoneService,
                                                CancellationToken cancellationToken)
        {

            _currentActionName = Request.Path.Value.Split('/').Last();
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString().ToLower();

            if (!ModelState.IsValid)
            {
                _responseCode = _modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            var userName = RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userName"].ToString();
            var currentUser = await userAccessor.GetUserByUserId((int)request.UserId, cancellationToken);

            if (currentUser == null)
            {
                _responseCode = String.Format("api:{0}:{1}:userNotFound", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            if (currentUser.Name != userName)
            {
                _logger.LogWarning($"The following User {userName} attempted to update record with UserName {currentUser.Name} and UserId {currentUser.UserId}");
                _responseCode = String.Format("api:{0}:{1}:unauthorizedToUpdateUser", _currentControllerName, _currentActionName);
                return StatusCode(403, new GenericApiResponse<string> { StatusCode = 403, ErrorCode = _responseCode });
            }

            var localTimeZones = await timeZoneService.GetTimeZones();
            if (!localTimeZones.Any(o => o.Id == request.TimeZone))
            {
                _responseCode = String.Format("api:{0}:{1}:incorrectTimeZone", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            var userToUpdate = new User
            {
                UserId = (int)request.UserId,
                Name = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailAddress = request.EmailAddress,
                TimeZone = request.TimeZone
            };

            var response = await _userAccountManagerService.UpdateUser(currentUser, userToUpdate, unitOfWorkFactory, cancellationToken);
            if (response.Status == UserAccountCreateStatus.UserExists)
            {
                return BadRequest(new GenericApiResponse<string> { StatusCode = 400, ErrorCode = $"api:{_currentControllerName}:{_currentActionName}:userAlreadyExists" });
            }

            return Ok(new GenericApiResponse<SuccessResponse> { StatusCode = 200, Data = new SuccessResponse("true") });
        }

        [HttpPost("sendPasswordResetLink")]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendPasswordResetLink([FromBody] SendPasswordLinkRequest request,
                                                               [FromServices] IUserMutator userMutator,
                                                               [FromServices] IUrlServices urlServices,
                                                               [FromServices] IUserAccessor userAccessor,
                                                               [FromServices] IEmailService emailService,
                                                               CancellationToken cancellationToken)
        {
            _currentActionName = Request.Path.Value.Split('/').Last();
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString().ToLower();

            if (!ModelState.IsValid)
            {
                _responseCode = _modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            //Does user exist
            var user = await userAccessor.GetUserByUserName(request.UserName, cancellationToken);

            //What if the TempPassword is already existing when a second request for same user is requested?
            //What if the emailAddress is empty?

            if (user == null)
            {
                _responseCode = String.Format("api:{0}:{1}:userNotFound", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            //Generate & Set New TempPassword & VerificationToken in Users table
            var passwordResetResult = await _userAccountManagerService.SetTempPasswordAndVerificationToken(user, cancellationToken);

            //Generate data for querystring
            var dataobject = JObject.FromObject(new { orgId = user.OrganizationId, userId = user.UserId, verificationToken = passwordResetResult.HashResult.Hash });
            var dataBytes = ASCIIEncoding.ASCII.GetBytes(dataobject.ToString());
            var data = Convert.ToBase64String(dataBytes);
            
            var url = String.Format("{0}/{1}?emailAddress={2}&data={3}", urlServices.GetReferrerUrl(this.HttpContext), "sendPasswordReset", request.UserName, data);

            //Build Email Message
            string messageBody = emailService.GenerateEmailMessage("PasswordReset", passwordResetResult.TempPassword, url);

            //Send Email
            var emailModel = new EmailModel(user.EmailAddress, "Compliance Investigate Password Reset", messageBody, true);
            emailService.SendEmail(emailModel, cancellationToken);

            _logger.LogInformation($"Password reset Email sent to {user.Name} with email address, { user.EmailAddress } and temporary password: { passwordResetResult.TempPassword }");

            return Ok(new GenericApiResponse<string> {StatusCode = 200});
        }


        [HttpPost("resetPassword")]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePassword([FromBody] ResetPasswordRequest request,
                                                               [FromServices] IUserMutator userMutator,
                                                               [FromServices] IUserAccessor userAccessor,
                                                               [FromServices] IPasswordService passwordService,
                                                               CancellationToken cancellationToken)
        {
            _currentActionName = Request.Path.Value.Split('/').Last();
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString().ToLower();

            if (!ModelState.IsValid)
            {
                _responseCode = _modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            //Does user exist
            var user = await userAccessor.GetUserByUserId(request.UserId, cancellationToken);

            if (user == null)
            {
                _logger.LogError($"User with UserId {request.UserId} does not exist");

                _responseCode = String.Format("api:{0}:{1}:updatePasswordFailed", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            if ((user.Name.ToLower() != request.UserName.ToLower()) || (user.UserId != request.UserId))
            {
                _logger.LogError($"User with UserId {user.UserId} and UserName {user.Name} does not match UserId {request.UserId} and UserName {request.UserName} in request");

                _responseCode = String.Format("api:{0}:{1}:updatePasswordFailed", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            //Encrypt incoming temp password
            var encryptedTempPwd = passwordService.EncryptPassword(request.TemporaryPassword);

            //Validate incoming encrypted temp password with encrypted temp password from user record
            if (encryptedTempPwd != user.TempPassword)
            {
                _logger.LogError($"TempPassword [{user.TempPassword}] for {request.UserName} does not match encrypted version of incoming password [{request.TemporaryPassword}]");

                _responseCode = String.Format("api:{0}:{1}:updatePasswordFailed", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            //Validate VerificationToken
            if (request.VerificationToken != user.VerificationToken)
            {
                _logger.LogError($"VerificationToken [{user.VerificationToken}] for {request.UserName} does not match incoming verificationToken [{request.VerificationToken}]");

                _responseCode = String.Format("api:{0}:{1}:updatePasswordFailed", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            //Update User's password to new password.
            await _userAccountManagerService.ResetUserPassword(user.UserId, request.NewPassword, cancellationToken);


            return Ok(new GenericApiResponse<string> { StatusCode = 200 });
        }

        [Authorize]
        [HttpPost("updatePassword")]
        [ProducesResponseType(typeof(GenericApiResponse<SuccessResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request,
                                                               [FromServices] IUserMutator userMutator,
                                                               [FromServices] IUserAccessor userAccessor,
                                                               [FromServices] JwtSignInHandler jwtHandler,
                                                               [FromServices] IPasswordService passwordService,
                                                               CancellationToken cancellationToken)
        {
            _currentActionName = Request.Path.Value.Split('/').Last();
            _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString().ToLower();

            if (!ModelState.IsValid)
            {
                _responseCode = _modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            User user;

            //Get UserName from JWT payload
            var userName = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userName"].ToString();

            if (!String.IsNullOrWhiteSpace(userName))
            {
                //Does User exist
                user = await userAccessor.GetUserByUserName(userName, cancellationToken);

                if (user == null)
                {
                    _logger.LogError($"User with UserName {userName} does not exist or was not authenticated.");

                    _responseCode = String.Format("api:{0}:{1}:invalidUser", _currentControllerName, _currentActionName);
                    return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
                }
            }
            else
            {
                _logger.LogError($"UserName with value of: {userName} from Jwt Payload was invalid");

                _responseCode = String.Format("api:{0}:{1}:invalidJwtPayload", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            //Encrypt incoming current password
            var encryptedCurrentPwd = passwordService.EncryptPassword(request.CurrentPassword);

            //Validate incoming encrypted currentPassword with encrypted Password from user record
            if (encryptedCurrentPwd != user.Password)
            {
                _logger.LogError($"Current Password [{user.Password}] for {user.Name} does not match encrypted version of incoming password [{request.CurrentPassword}]");

                _responseCode = String.Format("api:{0}:{1}:unableToUpdatePwd", _currentControllerName, _currentActionName);
                return StatusCode(400, new GenericApiResponse<string> { StatusCode = 400, ErrorCode = _responseCode });
            }

            var encryptedNewPwd = passwordService.EncryptPassword(request.NewPassword);
            await userMutator.UpdateUserPassword(user.UserId, encryptedNewPwd);

            return Ok(new GenericApiResponse<SuccessResponse> { StatusCode = 200, Data = new SuccessResponse("true") });
        }
    }
}
