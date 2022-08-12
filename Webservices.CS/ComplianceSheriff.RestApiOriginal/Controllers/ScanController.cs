using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ComplianceSheriff.RestApi.Requests;
using ComplianceSheriff.Work;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ComplianceSheriff.Scans;
using ComplianceSheriff.Requests;
using ComplianceSheriff.RestApi.WebResponse;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.Permission;
using ComplianceSheriff.UsageAudit;
using ComplianceSheriff.ModelState;
using ComplianceSheriff.CheckpointGroups;
using System.Linq;
using ComplianceSheriff.Runs;
using ComplianceSheriff.LogMessages;
using ComplianceSheriff.UserAgent;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    [Authorize]
    public class ScanController : Controller
    {
        private string _currentActionName;
        private string _currentControllerName;

        private readonly ILogger<ScanController> _logger;
        private readonly JwtSignInHandler _jwtSignInHandler;

        public ScanController(ILogger<ScanController> logger,
                              [FromServices] JwtSignInHandler jwtSignInHandler)
        {
            _logger = logger;
            _jwtSignInHandler = jwtSignInHandler;
        }

        #region "View/Get  Scan/ScanList"
        [HttpGet("scansList")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Scan>))]
        public async Task<IActionResult> GetScansList([FromServices] IScanAccessor accessor,
                                                      [FromServices] JwtSignInHandler jwtHandler,
                                                      CancellationToken cancellationToken)
        {
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());
            var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            var result = await accessor.GetScansList(userGroupId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("getById/{scanId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Scan>))]
        public async Task<IActionResult> GetScanById(string scanId,
                                                        CancellationToken cancellationToken,
                                                        [FromServices] IScanAccessor accessor,
                                                        [FromServices] IPermissionAccessor permissionAccessor,
                                                        [FromServices] IModelStateService modelStateService)
        {
            if (!ModelState.IsValid)
            {
                _currentActionName = Request.Path.Value.Split('/').Last();
                _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }
            try
            {
                var scanIdIsValid = double.TryParse(scanId, out double resultScanId);
                if (!scanIdIsValid)
                {
                    return BadRequest("api:scan:getById:invalidScanId");
                }

                var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
                var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());

                var hasViewPermission = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup((int)resultScanId, userGroupId, typeof(Scan).Name, "view", cancellationToken);

                var hasEditPermission = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup((int)resultScanId, userGroupId, typeof(Scan).Name, "edit", cancellationToken);

                if (!hasViewPermission && !hasEditPermission)
                {
                    return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = "api:scan:getScanById:noPermissionToView" });
                }

                var result = await accessor.GetScanById((int)resultScanId, userGroupId, cancellationToken);
                if (result == null)
                    return NotFound("api:scan:getScanById:scanNotFound");
                else
                    return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
                throw;
            }
        }

        [HttpGet("getByName/{scanName}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Scan>))]
        public async Task<IActionResult> GetScanByName([FromRoute] ScanByNameRequest request,
                                                        CancellationToken cancellationToken,
                                                        [FromServices] IScanAccessor accessor,
                                                        [FromServices] IPermissionAccessor permissionAccessor,
                                                        [FromServices] IModelStateService modelStateService)
        {
            if (!ModelState.IsValid)
            {
                _currentActionName = Request.Path.Value.Split('/').Last();
                _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }
            try
            {
                var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
                var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());

                List<Scan> result = await accessor.GetScanByName(request.scanName, cancellationToken);
                if (result == null || result.Count < 1)
                    return NotFound("api:scan:getScanByName:scanNotFound");
                else
                {
                    var hasViewPermission = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup(result[0].ScanId, userGroupId, typeof(Scan).Name, "view", cancellationToken);

                    var hasEditPermission = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup(result[0].ScanId, userGroupId, typeof(Scan).Name, "edit", cancellationToken);

                    if (!hasViewPermission && !hasEditPermission)
                    {
                        return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = "api:scan:getScanById:noPermissionToView" });
                    }
                    var scanResult = await accessor.GetScanById(result[0].ScanId, userGroupId, cancellationToken);
                    return Ok(scanResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
                throw;
            }
        }
        #endregion

        #region"Create Scan"
        [HttpPost("create")]
        [ProducesResponseType(201)]
        [Authorize(Roles = "ScanCreator")]
        public async Task<IActionResult> CreateScan([FromBody] ScanRequest request,
                                                    [FromServices] IScanMutator mutator,
                                                    [FromServices] IScanAccessor accessor,
                                                    [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                                    [FromServices] IModelStateService modelStateService,
                                                    [FromServices] ICheckpointGroupsAccessor chkGrpAccessor,
                                                    [FromServices] ICheckpointGroupService checkpointGroupService,
                                                    [FromServices] IPermissionService _permissionService,
                                                    [FromServices] IUsageAuditService usageAuditService,
                                                    [FromServices] IUserAgentAccessor userAgentAccessor,
                                                    CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _currentActionName = Request.Path.Value.Split('/').Last();
                _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }
            int scanId = 0;
            try
            {

                // Scan Exist?? -- Check and Get the ScanId for requested Scan Name.
                List<Scan> scanList = await accessor.GetScanByName(request.DisplayName, cancellationToken);
                if (scanList.Count() > 0)
                {
                    return BadRequest("api:scan:createScan:scanRecordAlreadyExist");
                }

                var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());
                var organizationId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();

                List<CheckpointGroupListItem> chkGroupList = await chkGrpAccessor.GetCheckpointGroupList(userGroupId, organizationId, cancellationToken);
                if (chkGroupList == null)
                {
                    return NotFound("api:scan:createScan:checkpointGroupListNotAvailable");
                }

                List<string> chkGroupIdList = new List<string>();
                foreach (CheckpointGroupListItem chkGrpItem in chkGroupList)
                    chkGroupIdList.Add(chkGrpItem.CheckpointGroupId);

                foreach (string chkGrpId in request.CheckpointGroupIds)
                {
                    if (!chkGroupIdList.Contains(chkGrpId))
                    {
                        _logger.LogInformation("createScan : The ScanCheckpointGroupId {0} does not exist.", chkGrpId.ToString());
                        return NotFound("api:scan:createScan:checkpointGroupIdNotAvailable");
                    }
                }
                var userAgent = await userAgentAccessor.GetUserAgentItem(request.UserAgent, organizationId, cancellationToken);
                request.UserAgent = userAgent.UserAgentString;
                request.StartPages = new List<StartPage>
                {
                    request.CreateStartPages()
                };
                request.DateCreated = request.DateCreated ?? DateTimeOffset.Now;
                request.DateModified = request.DateModified ?? DateTimeOffset.Now;
                scanId = await mutator.InsertScanAndDependencies(request, userGroupId, typeof(Scan).Name, "edit");
                if (scanId > 0)
                {
                    var scan = await accessor.GetScanById(scanId, userGroupId, cancellationToken);
                    var insertUsageRecord = accessor.GetScanRecordForAudit(scan);
                    using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                    {
                        usageAuditService.RecordUserAction(insertUsageRecord,
                                                            request.DisplayName,
                                                            "New Scan Created",
                                                            UserAuditType.Scans,
                                                            UserAuditActionType.Create,
                                                            HttpContext,
                                                            unitOfWork);

                        await unitOfWork.CommitAsync(cancellationToken);
                    }
                }
                else
                    return BadRequest("api:scan:createScan:scanIsNotCreated");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
                throw;
            }
            return StatusCode(201, new { ScanId = scanId });
        }

        #endregion

        #region "Edit/Update Scan And Delete Scan"
        [HttpDelete("delete/{scanId}")]
        [Authorize(Roles = "ScanEditor")]
        public async Task<IActionResult> DeleteScan(int scanId,
                                                          [FromServices] IScanAccessor accessor,
                                                          [FromServices] IScanMutator mutator,
                                                          [FromServices] IRunAccessor runAccessor,
                                                          [FromServices] IPermissionAccessor permissionAccessor,
                                                          [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                                          [FromServices] IModelStateService modelStateService,
                                                          [FromServices] IUsageAuditService usageAuditService,                                                          
                                                           CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _currentActionName = Request.Path.Value.Split('/').Last();
                _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }
            try
            {
                var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
                var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());

                //Check if a Scan definition exist for passed in ScanId 
                var scanExists = await accessor.CheckScanExistence(scanId, cancellationToken);

                if (!scanExists)
                    return NotFound("api:scan:deleteScan:scanNotFound");

                var scan = await accessor.GetScanById(scanId, userGroupId, cancellationToken);

                var hasPermissionToDelete = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup(scanId, userGroupId, typeof(Scan).Name, "edit", cancellationToken);
                if (!hasPermissionToDelete)
                {
                    return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = "api:scan:deleteScan:noPermissionToDeleteScan" });
                }

                int status = await runAccessor.GetRunStatusByScanId(scanId, cancellationToken);
                var runStatus = new RunStatus
                {
                    RunStatusValue = status,
                    RunStatusDescription = Enum.GetName(typeof(ScanRuns.RunStatus), status)
                };
                if (runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus), ScanRuns.RunStatus.Running).ToUpper().Trim() ||
                    runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus), ScanRuns.RunStatus.Pending).ToUpper().Trim() ||
                    runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus), ScanRuns.RunStatus.Pausing).ToUpper().Trim() ||
                    runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus), ScanRuns.RunStatus.Repairing).ToUpper().Trim() ||
                    runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus), ScanRuns.RunStatus.Resume).ToUpper().Trim())
                {
                    return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = "api:scan:deleteScan:scanInRunningOrPendingState" });
                }
                int resultViewId = await accessor.GetResultViewIdByScanId(scanId, cancellationToken);
                int lastCompletedRunId = await runAccessor.GetLastCompletedRunIdForScanId(scanId, cancellationToken);           


                // Below method does : Scan results cached page deletion, Runs Table update RunStatus to ScanDeleted, Resultview deletion, Health score and other related data deletion
                var delStatus = await mutator.DeleteScanAndDependencies(scanId, lastCompletedRunId);
                if (delStatus)
                {
                    using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                    {

                        var delRecord = accessor.GetScanRecordForAudit(scan);

                        usageAuditService.RecordUserAction(delRecord,
                                                            scan.DisplayName,
                                                            "Scan Deleted",
                                                            UserAuditType.Scans,
                                                            UserAuditActionType.Delete,
                                                            HttpContext,
                                                            unitOfWork);

                        await unitOfWork.CommitAsync(cancellationToken);
                    }

                    if (delStatus && lastCompletedRunId > 0)
                    {
                        bool fileDelStatus = mutator.DeleteScanResultFiles(lastCompletedRunId, orgId, cancellationToken);
                        if (!fileDelStatus)
                            _logger.LogInformation("deleteScan : The Scan Result File not able to delete for ScanId {0} .", scan.ScanId.ToString());
                    }
                    else
                        _logger.LogInformation("deleteScan : The Completed ScanRun not found for ScanId {0} .", scan.ScanId.ToString());
                }
               
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
                throw;
            }
            return Ok();
        }

        [HttpPost("Update")]
        [ProducesResponseType(201)]
        [Authorize(Roles = "ScanEditor")]
        public async Task<IActionResult> UpdateScan([FromBody] ScanUpdateRequest request,
                                                         [FromServices] IScanAccessor accessor,
                                                         [FromServices] IScanMutator mutator,
                                                         [FromServices] IPermissionAccessor permissionAccessor,
                                                         [FromServices] ICheckpointGroupsAccessor chkGrpAccessor,
                                                         [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                                         [FromServices] IModelStateService modelStateService,
                                                         [FromServices] IUsageAuditService usageAuditService,
                                                         [FromServices] IUserAgentAccessor userAgentAccessor,
                                                         [FromServices] IRunAccessor runAccessor,
                                                          CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _currentActionName = Request.Path.Value.Split('/').Last();
                _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }

            try
            {
                var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
                var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());
                var scanId = Convert.ToInt32(request.ScanId);

                // Scan Exist?? -- Check and Get the ScanId for requested Scan Name.
                var scanExists = await accessor.CheckScanExistence(scanId, cancellationToken);
                if (!scanExists)
                    return NotFound("api:scan:updateScan:scanDoesNotExist");
                var originalScan = await accessor.GetScanById(scanId, userGroupId, cancellationToken);             

                //Check if a User has permission to edit the scan
                var hasPermissionToUpdate = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup(scanId, userGroupId, typeof(Scan).Name, "edit", cancellationToken);
                if (!hasPermissionToUpdate)
                {
                    return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = "api:scan:updateScan:noPermissionToUpdate" });
                }

                int status = await runAccessor.GetRunStatusByScanId(scanId, cancellationToken);
                var runStatus = new RunStatus
                {
                    RunStatusValue = status,
                    RunStatusDescription = Enum.GetName(typeof(ScanRuns.RunStatus), status)
                };
                if (runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus),ScanRuns.RunStatus.Running).ToUpper().Trim() ||
                    runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus), ScanRuns.RunStatus.Pending).ToUpper().Trim() ||
                    runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus), ScanRuns.RunStatus.Pausing).ToUpper().Trim() ||
                    runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus), ScanRuns.RunStatus.Repairing).ToUpper().Trim() ||
                    runStatus.RunStatusDescription.ToUpper().Trim() == Enum.GetName(typeof(ScanRuns.RunStatus), ScanRuns.RunStatus.Resume).ToUpper().Trim())
                {
                    return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = "api:scan:updateScan:scanInRunningOrPendingState" });
                }

                List<CheckpointGroupListItem> chkGroupList = await chkGrpAccessor.GetCheckpointGroupList(userGroupId, orgId, cancellationToken);
                if (chkGroupList == null)
                {
                    return NotFound("api:scan:updateScan:checkpointGroupListNotAvailable");
                }

                List<string> chkGroupIdList = new List<string>();
                foreach (CheckpointGroupListItem chkGrpItem in chkGroupList)
                    chkGroupIdList.Add(chkGrpItem.CheckpointGroupId);

                foreach (string chkGrpId in request.CheckpointGroupIds)
                {
                    if (!chkGroupIdList.Contains(chkGrpId))
                    {
                        _logger.LogInformation("editScan : The ScanCheckpointGroupId {0} does not exist.", chkGrpId.ToString());
                        return NotFound("api:scan:updateScan:checkpointGroupIdNotAvailable");
                    }
                }
                var userAgent = await userAgentAccessor.GetUserAgentItem(request.UserAgent, orgId, cancellationToken);
                request.UserAgent = userAgent.UserAgentString;
                request.StartPages = new List<StartPage>
                {
                    request.CreateStartPages()
                };

                //if (!originalScan.Equals((Scan)request))
                //{
                    request.DateModified = DateTimeOffset.Now;
                    bool editScanStatus = await mutator.UpdateScanAndCheckpointGroups(scanId, request);
                    if (editScanStatus)
                    {
                        var updatedScan = await accessor.GetScanById(scanId, userGroupId, cancellationToken);
                        var updatedRecord = accessor.GetScanChangesForAudit(originalScan, updatedScan, chkGroupList);
 
                        using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
                        {
                            usageAuditService.RecordUserAction(updatedRecord,
                                                                request.DisplayName,
                                                                originalScan.Equals((Scan)request) ? "Scan had no changes" : "Scan modified",
                                                                UserAuditType.Scans,
                                                                UserAuditActionType.Modify,
                                                                HttpContext,
                                                                unitOfWork);

                            await unitOfWork.CommitAsync(cancellationToken);
                        }
                    }
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} {ex.StackTrace}");
                throw;
            }
            return Ok();
        }

        #endregion

        #region "RunScan, AbortScan and GetScanStatus"

        [HttpPut("abort/{runId}")]
        [Authorize(Roles = "ScanEditor")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AbortScan(string runId,
                                                   [FromServices] IRunAccessor accessor,
                                                   [FromServices] IScanMutator scanMutator,
                                                   [FromServices] IScanAccessor scanAccessor,
                                                   [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                                   [FromServices] IUsageAuditService usageAuditService,
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
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());
            var runIdIsValid = Int32.TryParse(runId, out int resultRunId);
            if (!runIdIsValid)
            {
                return BadRequest("api:scan:abort:invalidRunId");
            }

            //Check if a Run with runId passed in exists
            var run = await accessor.GetRunByRunId(resultRunId, cancellationToken);

            if (run == null)
            {
                return NotFound("api:scan:abort:runNotFound");
            }

            //Check the status of the latest runId from the scanId passed in
            if (run.Status != 1 && run.Status != 4)
            {
                return BadRequest("api:scan:abort:unableToAbortScanDueToCurrentStatus");
            }

            var abortReason = String.Format("Stopped by user");
            await scanMutator.AbortScanRun(resultRunId, (int)ScanRuns.RunStatus.Aborted, abortReason);

            var scan = await scanAccessor.GetScanById(run.ScanId, userGroupId, cancellationToken);
            using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
            {
                usageAuditService.RecordUserAction("",
                                                    scan.DisplayName,
                                                    "Scan stopped",
                                                    UserAuditType.Scans,
                                                    UserAuditActionType.Stop,
                                                    HttpContext,
                                                    unitOfWork);

                await unitOfWork.CommitAsync(cancellationToken);
            }

            return Ok();
        }

        [HttpPost("run")]
        [Authorize(Roles = "ScanEditor")]
        [ProducesResponseType(201)]
        public async Task<IActionResult> RunScan([FromBody] ScanRunRequest request,
                                                    [FromServices] IScanAccessor scanAccessor,
                                                    [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
                                                    [FromServices] IPermissionAccessor permissionAccessor,
                                                    [FromServices] IUsageAuditService usageAuditService,
                                                    [FromServices] IScanService scanService,
                                                    [FromServices] IRunAccessor runAccessor,
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

            var scanIdIsValid = Int32.TryParse(request.ScanId, out int scanId);
            if (!scanIdIsValid)
            {
                return BadRequest("api:scan:runScan:invalidScanId");
            }

            var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());

            //Check if a Scan with ScanId passed in exists
            var scanExists = await scanAccessor.CheckScanExistence(scanId, cancellationToken);

            if (!scanExists)
            {
                return NotFound("api:scan:runScan:scanNotFound");
            }

            var hasPermissionToRun = await permissionAccessor.CheckScanRunPermission(Convert.ToInt32(request.ScanId), userGroupId, cancellationToken);
            if (!hasPermissionToRun)
            {
                return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = "api:scan:run:noPermissionToRun" });
            }

            //Check the status of the latest runId from the scanId passed in
            var status = await runAccessor.GetRunStatusByScanId(scanId, cancellationToken);
            if (status != 0 && status != 2 && status != 3)
            {
                return BadRequest("api:scan:runScan:unableToRunScanDueToCurrentStatus");
            }

            int runId = await scanService.RunScan(orgId, scanId, null, "", unitOfWorkFactory, cancellationToken);

            var scan = await scanAccessor.GetScanById(scanId, userGroupId, cancellationToken);

            using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
            {
                usageAuditService.RecordUserAction("",
                                                    scan.DisplayName,
                                                    "Scan run",
                                                    UserAuditType.Scans,
                                                    UserAuditActionType.Run,
                                                    HttpContext,
                                                    unitOfWork);

                await unitOfWork.CommitAsync(cancellationToken);
            }

            return StatusCode(201, new { RunId = runId });
        }

        [HttpGet("runStatus/{scanId}")]
        [ProducesResponseType(200, Type = typeof(RunStatus))]
        public async Task<IActionResult> RunScanStatus(string scanId,
                                                       [FromServices] IRunAccessor runAccessor,
                                                       [FromServices] IScanAccessor scanAccessor,
                                                       [FromServices] IModelStateService modelStateService,
                                                       [FromServices] IPermissionAccessor permissionAccessor,
                                                       CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _currentActionName = Request.Path.Value.Split('/').Last();
                _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }

            var scanIdIsValid = double.TryParse(scanId, out double resultScanId);
            if (!scanIdIsValid)
            {
                return BadRequest("api:scan:runStatus:invalidScanId");
            }

            var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());

            //Check if a Scan with ScanId passed in exists
            var scanExists = await scanAccessor.CheckScanExistence((int)resultScanId, cancellationToken);

            if (!scanExists)
            {
                return NotFound("api:scan:runStatus:scanNotFound");
            }

            var hasViewPermission = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup((int)resultScanId, userGroupId, typeof(Scan).Name, "view", cancellationToken);
            var hasEditPermission = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup((int)resultScanId, userGroupId, typeof(Scan).Name, "edit", cancellationToken);
           
            if (!hasViewPermission && !hasEditPermission)
            {
                return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = "api:scan:runStatus:noPermissionToView" });
            }

            var status = await runAccessor.GetRunStatusByScanId((int)resultScanId, cancellationToken);

            var runStatus = new RunStatus
            {
                RunStatusValue = status,
                RunStatusDescription = Enum.GetName(typeof(ScanRuns.RunStatus), status)
            };

            return Ok(runStatus);
        }

        #endregion

        #region "Get RunScan Log Details"
        [HttpGet("scanRunLog/{scanId}/{runId}")]
        [ProducesResponseType(200, Type = typeof(List<LogMessagesItem>))]
        public async Task<IActionResult> GetScanRunLog(string scanId, string runId,
                                                       [FromServices] IRunAccessor runAccessor,
                                                       [FromServices] ILogMessagesAccessor logMessagesAccessor,
                                                       [FromServices] IScanAccessor scanAccessor,
                                                       [FromServices] IModelStateService modelStateService,
                                                       [FromServices] IPermissionAccessor permissionAccessor,
                                                       CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _currentActionName = Request.Path.Value.Split('/').Last();
                _currentControllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var responseCode = modelStateService.InvalidResponseHandler(ModelState, _currentControllerName, _currentActionName);
                return StatusCode(400, new ApiResponse { StatusCode = 400, ErrorCode = responseCode });
            }
            int resultScanId = 0;
            int resultRunId = 0;
            var scanIdIsValid = string.IsNullOrEmpty(scanId) ? false : Int32.TryParse(scanId, out resultScanId);
            if (!scanIdIsValid)
            {
                return BadRequest("api:scan:getScanRunLog:invalidScanId");
            }

            var runIdIsValid = string.IsNullOrEmpty(runId) ? false : Int32.TryParse(runId, out resultRunId);
            if (!runIdIsValid)
            {
                return BadRequest("api:scan:getScanRunLog:invalidRunId");
            }

            var orgId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["userGroupId"].ToString());

            //Check if a Scan with ScanId passed in exists
            var scanExists = await scanAccessor.CheckScanExistence(resultScanId, cancellationToken);

            if (!scanExists)
            {
                return NotFound("api:scan:getScanRunLog:scanNotFound");
            }

            var hasViewPermission = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup((int)resultScanId, userGroupId, typeof(Scan).Name, "view", cancellationToken);
            var hasEditPermission = await permissionAccessor.CheckPermissionTypeAndActionForUserGroup((int)resultScanId, userGroupId, typeof(Scan).Name, "edit", cancellationToken);

            if (!hasViewPermission && !hasEditPermission)
            {
                return StatusCode(403, new ApiResponse { StatusCode = 403, ErrorCode = "api:scan:getScanRunLog:noPermissionToView" });
            }
            var run = await runAccessor.GetRunByRunId(resultRunId, cancellationToken);
            if (run == null)
                return NotFound("api:scan:getScanRunLog:runNotFound");
            else if (run.ScanId != resultScanId)
                return NotFound("api:scan:getScanRunLog:runNotFoundForProvidedScanId");

            string loggerRunId = "Run." + resultRunId.ToString();
            var result = await logMessagesAccessor.GetLogMessagesRecord(loggerRunId, cancellationToken);
            if (result == null)
                return NoContent();
            else if (result.Count < 1)
                return NoContent();
            else
                return Ok(result);
        }
        #endregion  
    }
}