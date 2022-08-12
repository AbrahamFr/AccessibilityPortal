using ComplianceSheriff.Attributes;
using ComplianceSheriff.AuditReports;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.Configuration;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.Requests;
using ComplianceSheriff.UsageAudit;
using ComplianceSheriff.Permission;
using ComplianceSheriff.Work;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ComplianceSheriff.RestApi.WebResponse;
using System.Text.RegularExpressions;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    [Authorize(Roles = "AuditReport")]
    public class AuditReportController : Controller
    {
        private readonly ILogger<AuditReportController> _logger;
        private readonly ConfigurationOptions _configOptions;
        private readonly JwtSignInHandler _jwtSignInHandler;
        private readonly FormOptions _defaultFormOptions = new FormOptions();
        private readonly UserAuditType _userAuditType = UserAuditType.ManualAudits;
        private readonly IUsageAuditMutator _usageMutator;
        private readonly IUsageAuditService _usageAuditService;
        private readonly IPermissionService _permissionService;
        private readonly IPermissionAccessor _permissionAccessor;
        private string responseCode = String.Empty;

        public AuditReportController(ILogger<AuditReportController> logger,
                                     IOptions<ConfigurationOptions> options,
                                     [FromServices] IUsageAuditMutator usageMutator,
                                     [FromServices] IUsageAuditService usageAuditService,
                                     [FromServices] JwtSignInHandler jwtSignInHandler,
                                     [FromServices] IPermissionService permissionService,
                                     [FromServices] IPermissionAccessor permissionAccessor)
        {
            _logger = logger;
            _configOptions = options.Value;
            _jwtSignInHandler = jwtSignInHandler;
            _usageAuditService = usageAuditService;
            _usageMutator = usageMutator;
            _permissionService = permissionService;
            _permissionAccessor = permissionAccessor;
        }

        [HttpDelete("delete/{auditReportId}")]
        [Authorize(Roles = "AuditReportEditor")]
        public async Task<IActionResult> DeleteAuditReport(int auditReportId,
                                                           [FromServices] IAuditReportAccessor accessor,
                                                           [FromServices] IAuditReportMutator mutator,
                                                           [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                                           CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Check if Record exists
                var userName = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userName"].ToString();
                var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());
                var auditReportResult = await accessor.GetAuditReportWithUserPermission(userGroupId, auditReportId, cancellationToken);

                if (auditReportResult == null)
                {
                    using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                    {
                        _usageAuditService.RecordUserAction("Record Not Found",
                                                                  "Record Not Found",
                                                                   UserAuditActivityMessage.FileNotFound,
                                                                   _userAuditType,
                                                                   UserAuditActionType.Delete,
                                                                   HttpContext,
                                                                   unitOfWork);

                        await unitOfWork.CommitAsync(cancellationToken);
                    }

                    responseCode = "api:auditReport:delete:recordNotFound";
                    _logger.LogError($"{responseCode} : No record found with AuditReportId: {auditReportId}");
                    return StatusCode(404, new ApiResponse { StatusCode = 404, ErrorCode = responseCode });
                }


                //Check if User has permissions

                if (!auditReportResult.CanEdit)
                {
                    using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                    {
                        _usageAuditService.RecordUserAction("User Forbidden to delete",
                                                                  "User Forbidden to delete",
                                                                   UserAuditActivityMessage.UnAuthorizeLogin,
                                                                   _userAuditType,
                                                                   UserAuditActionType.Delete,
                                                                   HttpContext,
                                                                   unitOfWork);

                        await unitOfWork.CommitAsync(cancellationToken);
                    }

                    responseCode = "api:auditReport:delete:noPermission";
                    _logger.LogError($"{responseCode} : {userName} unable to delete AuditReportId: {auditReportId}");
                    return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = responseCode });
                }
 
                var fileDeleted = RestAPIUtils.DeleteFiles(auditReportResult.FileLocation);

                if(!fileDeleted)
                {
                    _logger.LogInformation($"File {auditReportResult.FileLocation} was not removed. [AuditReportId: {auditReportResult.AuditReportId}, Report Description: {auditReportResult.ReportDescription}]");
                }
                    
                using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                {
                    mutator.DeleteAuditReport(auditReportId, userGroupId, unitOfWork);

                    _usageAuditService.RecordUserAction(Path.GetFileName(auditReportResult.FileLocation),
                                                                    auditReportResult.ReportName,
                                                                    UserAuditActivityMessage.Delete,
                                                                    _userAuditType,
                                                                    UserAuditActionType.Delete,
                                                                    HttpContext,
                                                                    unitOfWork);

                    await unitOfWork.CommitAsync(cancellationToken);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.StackTrace);
                throw;
            }
        }

        [HttpGet("AuditReportsList")]
        [Authorize(Roles = "AuditReportViewer")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuditReport>))]
        public async Task<IActionResult> GetAllAuditReportsList([FromServices] IAuditReportAccessor accessor, CancellationToken cancellationToken)
        {
            //string userName = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userName"].ToString();
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());
            var result = await accessor.GetAllAuditReports(userGroupId, cancellationToken);

            return Ok(result);
        }

        [HttpPut("edit")]
        [Authorize(Roles = "AuditReportEditor")]
        [ProducesResponseType(200, Type = typeof(IActionResult))]
        public async Task<IActionResult> AuditReportEdit([FromBody] AuditReportEditRequest request,
                                                         [FromServices] IAuditReportAccessor accessor,
                                                         [FromServices] IAuditReportMutator auditReportMutator, 
                                                         [FromServices] IUnitOfWorkFactory unitOfWorkFactory, 
                                                         CancellationToken cancellationToken)
        {          
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!(request.AuditTypeId > 0 && request.AuditTypeId < 5))
                {
                    responseCode = "api:auditReport:edit:invalidReportType";
                    _logger.LogError($"Invalid value passed in Audit Report Edit for reportType: {request.AuditTypeId}.");
                    return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
                }

                //Check if Record exists
                var userName = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userName"].ToString();
                var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());
                var auditReportResult = await accessor.GetAuditReportWithUserPermission(userGroupId, request.AuditReportId, cancellationToken);

                //Return 404 if record is not found
                if (auditReportResult == null)
                {
                    responseCode = "api:auditReport:edit:recordNotFound";
                    _logger.LogError($"{responseCode} : No record found with AuditReportId: {request.AuditReportId}");
                    return StatusCode(404, new ApiResponse { StatusCode = 404, ErrorCode = responseCode });
                }

                if (!auditReportResult.CanEdit)
                {
                    responseCode = "api:auditReport:edit:noPermission";
                    _logger.LogError($"{responseCode} : {userName} unable to edit AuditReportId: {request.AuditReportId}");
                    return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = responseCode });
                }

                //Check if ReportName already exists
                var result = await accessor.GetAuditReportsByReportName(request.ReportName, cancellationToken);

                if (result.Any(a => a.ReportName.ToLower().Trim() == request.ReportName.ToLower().Trim() && a.AuditReportId != request.AuditReportId))
                {
                    responseCode = "api:auditReport:edit:reportNameExists";
                    _logger.LogError($"Record Name already exists with ReportName: {request.ReportName}");
                    return StatusCode(409, new ApiResponse { StatusCode = 409, ErrorCode = responseCode });
                }

                //Update record in database
                using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                {
                    var auditReport = new AuditReport
                    {
                        AuditReportId = request.AuditReportId,
                        AuditTypeId = request.AuditTypeId,
                        ReportName = request.ReportName,
                        ReportDescription = request.ReportDescription
                    };

                    auditReportMutator.UpdateAuditReport(auditReport, unitOfWork);
                    

                    var changes = String.Format("ReportName: {0}\r\n" +
                                                "ReportDescription: {1}\r\n" +
                                                "ReportType: {2} ({3})\r\n" +
                                                "AuditReportId: {4}",
                                                request.ReportName,
                                                request.ReportDescription,
                                                Enum.GetName(typeof(Enums.AuditReportTypes), request.AuditTypeId),
                                                request.AuditTypeId,
                                                request.AuditReportId);

                    _usageAuditService.RecordUserAction(changes,
                                                                    request.ReportName,
                                                                    UserAuditActivityMessage.Modify,
                                                                    _userAuditType,
                                                                    UserAuditActionType.Modify,
                                                                    HttpContext,
                                                                    unitOfWork);

                    await unitOfWork.CommitAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
                throw;
            }

            return Ok();
        }


        [HttpPost("upload")]
        [DisableFormValueModelBinding]
        [SwaggerOperationFilter(typeof(Filters.UploadOperationFilter))]
        [RequestSizeLimit(105_000_000)]
        [Authorize(Roles = "AuditReportCreator")]        
        [ProducesResponseType(201, Type = typeof(IActionResult))]
        public async Task<IActionResult> AuditUploadStream([FromServices] IAuditReportMutator auditReportMutator, 
                                                           [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                                           [FromServices] IAuditReportAccessor accessor,
                                                           CancellationToken cancellationToken)
        {
            FormValueProvider formModel;

            //Retrieve fileName from QueryString
            var fileNameFromQueryString = HttpContext.Request.Query["fileName"].ToString().Trim();

            var fullFilePath = AuditReportService.BuildAuditFilePath(HttpContext, fileNameFromQueryString, this._configOptions, _jwtSignInHandler);
            _logger.LogInformation(fullFilePath);
           
            //Return a 409-Conflict error when file already exists
            if (System.IO.File.Exists(fullFilePath))
            {
                using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                {
                    _usageAuditService.RecordUserAction(Path.GetFileName(fullFilePath),
                                                           string.Empty,
                                                           UserAuditActivityMessage.Exists,
                                                           _userAuditType,
                                                           UserAuditActionType.Upload,
                                                           HttpContext,
                                                           unitOfWork);

                    await unitOfWork.CommitAsync(cancellationToken);
                }

                responseCode = "api:auditReport:upload:fileNameExists";
                _logger.LogError($"{Path.GetFileName(fullFilePath)} already exists.");
                return StatusCode(409, new ApiResponse { StatusCode = 409, ErrorCode = responseCode });
            }

            try
            {
                //Creates full path if it does not exist
                var filePath = fullFilePath.Substring(0, fullFilePath.LastIndexOf('\\'));
                Directory.CreateDirectory(filePath);

                using (var stream = System.IO.File.Create(fullFilePath))
                {
                    formModel = await FileStreamingHelper.StreamFile(Request, stream);
                }
                
                //Only process if the file upload succeeds
                if (formModel != null)
                {
                    //Validate form inputs
                    var validateResponse = AuditReportService.ValidateUploadFormInputs(formModel, fileNameFromQueryString);
                    if (!validateResponse.IsValid)
                    {
                        //If file exists, then delete file from shared directory
                        if (System.IO.File.Exists(fullFilePath))
                        {
                            System.IO.File.Delete(fullFilePath);
                            _logger.LogInformation($"The File {fullFilePath} was deleted due to {validateResponse.Reason}");
                        }

                        _logger.LogError(validateResponse.LogErrorMessage);
                        return StatusCode(validateResponse.ApiResponse.StatusCode, validateResponse.ApiResponse);
                    }

                    var auditReportRecord = AuditReportService.BuildNewAuditReportRecord(fullFilePath, formModel);

                    //check for existing record with matching ReportName
                    //If record does exist for incoming ReportName, 
                    //then delete the file and do not insert the record.
                    var existingAuditReportRecord = await accessor.GetAuditReportsByReportName(auditReportRecord.ReportName, cancellationToken);

                    if (existingAuditReportRecord.Any())
                    {
                        //If file exists, then delete file from shared directory
                        if (System.IO.File.Exists(fullFilePath) && !existingAuditReportRecord.Select(r => r.FileLocation.Substring(r.FileLocation.LastIndexOf("\\")+1)).Contains(fileNameFromQueryString))
                        {
                            System.IO.File.Delete(fullFilePath);
                            _logger.LogInformation("The File {0} was deleted due to existing 'ReportName' in the AuditReport table", fullFilePath);
                        }

                        _logger.LogInformation("The ReportName {0} already exists in the AuditReport table", auditReportRecord.ReportName);

                        responseCode = "api:auditReport:upload:reportNameExists";
                        _logger.LogError($"Record Name already exists with ReportName: {auditReportRecord.ReportName}");
                        return StatusCode(409, new ApiResponse { StatusCode = 409, ErrorCode = responseCode });
                    }
                    
                    //Insert new record into database
                    using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                    {
                        auditReportMutator.AddAuditReport(auditReportRecord, unitOfWork);

                        var uploadInfo = String.Format("FileName: {0}\r\n" +
                                                       "ReportName: {1}\r\n" +
                                                       "ReportDescription: {2}\r\n" +
                                                       "ReportType: {3} ({4})\r\n",
                                                        Path.GetFileName(auditReportRecord.FileLocation),
                                                        formModel.GetValue("reportName").ToString().Trim(),
                                                        formModel.GetValue("reportDescription").ToString(),
                                                        Enum.GetName(typeof(Enums.AuditReportTypes), Convert.ToInt32(formModel.GetValue("reportType").ToString().Trim())),
                                                        formModel.GetValue("reportType").ToString().Trim());

                        //Record User Action into UsageAudit table
                        _usageAuditService.RecordUserAction(uploadInfo,
                                                            auditReportRecord.ReportName,
                                                            UserAuditActivityMessage.Uploaded,
                                                            _userAuditType,
                                                            UserAuditActionType.Upload,
                                                            HttpContext,
                                                            unitOfWork);

                        await unitOfWork.CommitAsync(CancellationToken.None);

                        _logger.LogInformation("The audit entry for report {0} successfully added in [UserAudit] table.", auditReportRecord.ReportName);
                        _logger.LogInformation("The report {0} successfully added in [AuditReports] table.", auditReportRecord.ReportName);

                    }

                    // Get the AuditReportId for created AuditReportFile.
                    Int32 auditReportId = await accessor.GetAuditReportId(auditReportRecord.ReportName, auditReportRecord.FileLocation, cancellationToken);

                    //  Below code adds permission to created/uploaded audit report files.
                    string userName = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userName"].ToString();
                    var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());
                    //Int32 userGroupId = await _permissionAccessor.GetUserGroupIdForPermission(userName, cancellationToken);

                    //Record User Permissions
                    using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                    {
                        await _permissionService.RecordUserPermission(userGroupId, typeof(AuditReport).Name, auditReportId.ToString(), "edit", unitOfWork);
                    }

                    _logger.LogInformation("The permission to UserGroupId {0} for AuditReportId {1} successfully added in [UserGroupPermissions] table.", auditReportRecord.ReportName, auditReportId.ToString());
                }
            }
            catch(BadHttpRequestException ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");

                //If file exists, then delete file from shared directory
                if (System.IO.File.Exists(fullFilePath))
                {
                    System.IO.File.Delete(fullFilePath);
                    _logger.LogInformation($"The File {fullFilePath} was deleted due to {ex.Message}");
                }

                responseCode = "api:auditReport:upload:fileRequestTooLarge";
                _logger.LogError($"{ex.Message} {DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")}");
                return StatusCode(400, new ApiResponse { StatusCode = 409, ErrorCode = responseCode });
            }
            catch(IOException ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");

                //If file exists, then delete file from shared directory
                if (System.IO.File.Exists(fullFilePath))
                {
                    System.IO.File.Delete(fullFilePath);
                    _logger.LogInformation($"The File {fullFilePath} was deleted due to {ex.Message}");
                }

                responseCode = "api:auditReport:upload:invalidUploadFileName";
                _logger.LogError($"{ex.Message} {DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")}");
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");

                //If file exists, then delete file from shared directory
                if (System.IO.File.Exists(fullFilePath))
                {
                    System.IO.File.Delete(fullFilePath);
                    _logger.LogInformation("The File {0} was deleted due to error processing upload request", fullFilePath);
                }

                throw;
            }
 
            return Created(HttpContext.Request.Path, null);
        }
        
        [HttpGet("download/{auditReportId}")]
        [Authorize(Roles = "AuditReportViewer")]
        public async Task<IActionResult> AuditDownload(int auditReportId, 
                                                       [FromServices] IUnitOfWorkFactory unitOfWorkFactory, 
                                                       [FromServices] IAuditReportAccessor accessor, 
                                                       [FromServices] IAuditReportMutator mutator,
                                                       CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await accessor.GetAuditReportById(
                    auditReportId: auditReportId,
                    cancellationToken: cancellationToken);

            if(result == null)
            {
                responseCode = "api:auditReport:download:recordNotFound";             
                _logger.LogError($"{responseCode} : No record found with AuditReportId: {auditReportId}");
                
                return StatusCode(404, new ApiResponse { StatusCode = 404, ErrorCode = responseCode });
            }
           
            //If file does not exist on the server, then we return NoContent Response
            if (!System.IO.File.Exists(result.FileLocation))
            {
                //Update AuditReport record with fileStatus of Missing - [Id 2]
                using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                {
                    mutator.UpdateAuditReportFileStatus(auditReportId: auditReportId,
                                                                       fileStatusId: 2,
                                                                       unitOfWork: unitOfWork);

                    _usageAuditService.RecordUserAction(Path.GetFileName(result.FileLocation),
                                                              result.ReportName,
                                                              UserAuditActivityMessage.FileNotFound,
                                                              _userAuditType,
                                                              UserAuditActionType.Download,
                                                              HttpContext,
                                                              unitOfWork);

                    await unitOfWork.CommitAsync(cancellationToken);
                }

                responseCode = "api:auditReport:download:fileNotFound";
                _logger.LogError($"{responseCode} : {result.FileLocation} not found on the server");
                return StatusCode(404, new ApiResponse { StatusCode = 404, ErrorCode = responseCode });
            }

            var stream = new FileStream(result.FileLocation, FileMode.Open);

            using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
            {
                _usageAuditService.RecordUserAction(Path.GetFileName(result.FileLocation),
                                                        result.ReportName,
                                                        UserAuditActivityMessage.Downloaded,
                                                        _userAuditType,
                                                        UserAuditActionType.Download,
                                                        HttpContext,
                                                        unitOfWork);

                await unitOfWork.CommitAsync(cancellationToken);

            }
            
            return File(stream, AuditReportService.GetContentType(result.FileLocation), Path.GetFileName(result.FileLocation));
        }
    }
}
