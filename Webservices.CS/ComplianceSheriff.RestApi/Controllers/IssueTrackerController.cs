using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ComplianceSheriff.IssueTrackerReport;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using ComplianceSheriff.RestApi.WebResponse;
using System;
using ComplianceSheriff.ExportFormatter;
using Swashbuckle.AspNetCore.Annotations;
using ComplianceSheriff.UsageAudit;
using ComplianceSheriff.Work;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.ModelState;
using ComplianceSheriff.Urls;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    [Authorize(Roles = "IssueTrackerReport")]
    public class IssueTrackerController : Controller
    {
        private readonly ILogger<IssueTrackerController> _logger;
        private readonly IUsageAuditService _usageAuditService;
        private readonly JwtSignInHandler _jwtSignInHandler;
        private readonly IUrlServices _urlServices;
        private readonly UserAuditType _userAuditType = UserAuditType.IssueTracker;
        private string _currentActionName;
        private string _currentControllerName;

        public IssueTrackerController(ILogger<IssueTrackerController> logger,
                                      [FromServices] IUsageAuditService usageAuditService,                                      
                                      [FromServices] JwtSignInHandler jwtSignInHandler,
                                      [FromServices] IUrlServices urlServices)
        {
            _logger = logger;
            _urlServices = urlServices;
            _jwtSignInHandler = jwtSignInHandler;
            _usageAuditService = usageAuditService;           
        }

        [HttpPost("IssueTrackerList")]
        [ProducesResponseType(200, Type = typeof(IssueTrackerResponse))]
        public async Task<IActionResult> RetrieveIssueList([FromBody] IssueTrackerRequest request, 
                                                           [FromServices] IIssueTrackerAccessor accessor,
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
            var organizationId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            var result = await accessor.GetIssueTrackerResults(userGroupId, organizationId, request, cancellationToken);

            return Ok(result);
        }

        [HttpPost("IssueTrackerExport")]
        [SwaggerResponse(200, Type = typeof(FileContentResult))]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public async Task<IActionResult> ExportIssueList([FromBody] IssueTrackerExportRequest request, 
                                                         [FromServices] IIssueTrackerAccessor accessor,
                                                         [FromServices] IModelStateService modelStateService,                                                         
                                                         [FromServices] IUnitOfWorkFactory unitOfWorkFactory,
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
            var organizationId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();

            var result = await accessor.GetIssueTrackerResults(userGroupId, organizationId, (IssueTrackerRequestBase)request, cancellationToken);

            var contentType = String.Format("text/{0}", request.ExportFormat.ToString());
            var fileName = String.Format("{0}.{1}", request.FileName, request.ExportFormat.ToString());

            using (var unitOfWork = unitOfWorkFactory.CreateUnitOfWork())
            {
                _usageAuditService.RecordUserAction(fileName,
                                                    fileName,
                                                    UserAuditActivityMessage.Export,
                                                    _userAuditType,
                                                    UserAuditActionType.Export,
                                                    HttpContext,
                                                    unitOfWork);

                await unitOfWork.CommitAsync(cancellationToken);
            }

            var format = request.ExportFormat.ToString();
            IFileExporter<IssueTrackerReportItem> exporter = ExportFormatFactory<IssueTrackerReportItem>.CreateFormatter(format, result.IssueTrackerList);
            var output = exporter.Export("IssueTracker");
            
            return File(output, "application/octet-stream", fileName);
        }

        [HttpPost("OccurrencesList")]
        [ProducesResponseType(200, Type = typeof(OccurrencesResponse))]
        public async Task<IActionResult> RetrieveOccurrences([FromBody] OccurrencesRequest request,
                                                             [FromServices] IModelStateService modelStateService,
                                                             [FromServices] IIssueTrackerAccessor accessor, 
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
            var organizationId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            var orgVirtualDirectory = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["orgVirtualDir"].ToString();
            request.ReferrerUrl = _urlServices.GetReferrerUrl(this.HttpContext, orgVirtualDirectory);

            var result = await accessor.GetOccurrences(userGroupId, organizationId, request, cancellationToken);

            return Ok(result);
        }

        [HttpPost("OccurrencesByPage")]
        [ProducesResponseType(200, Type = typeof(OccurrencesByPageResponse))]
        public async Task<IActionResult> RetrieveOccurrencesByPage([FromBody] OccurrencesByPageRequest request,
                                                                   [FromServices] IModelStateService modelStateService,
                                                                   [FromServices] IIssueTrackerAccessor accessor,
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
            var organizationId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            var orgVirtualDirectory = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["orgVirtualDir"].ToString();
            request.ReferrerUrl = _urlServices.GetReferrerUrl(this.HttpContext, orgVirtualDirectory);

            var result = await accessor.GetOccurrencesByPage(userGroupId, organizationId, request, cancellationToken);

            return Ok(result);
        }

        [HttpPost("OccurrencesExport")]
        [SwaggerResponse(200, Type = typeof(FileContentResult))]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public async Task<IActionResult> ExportOccurrencesList([FromBody] OccurrencesExportRequest request,
                                                               [FromServices] IModelStateService modelStateService,
                                                               [FromServices] IIssueTrackerAccessor accessor,
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
            var organizationId = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["organizationId"].ToString();
            var orgVirtualDirectory = RestAPIUtils.GetJwtPayload(HttpContext, _jwtSignInHandler)["orgVirtualDir"].ToString();
            request.ReferrerUrl = _urlServices.GetReferrerUrl(this.HttpContext, orgVirtualDirectory);

            var result = await accessor.GetOccurrencesExport(userGroupId, organizationId, request, cancellationToken);

            var contentType = String.Format("text/{0}", request.ExportFormat.ToString());
            var fileName = String.Format("{0}.{1}", request.FileName, request.ExportFormat.ToString());

            var format = request.ExportFormat.ToString();
            IFileExporter<OccurrencesExportItem> exporter = ExportFormatFactory<OccurrencesExportItem>.CreateFormatter(format, result);
            var output = exporter.Export("Occurrences");

            return File(output, "application/octet-stream", fileName);
        }
    }


}
