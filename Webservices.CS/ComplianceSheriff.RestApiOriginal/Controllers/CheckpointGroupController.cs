using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ComplianceSheriff.CheckpointGroups;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Authorization;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.Extensions;
using ComplianceSheriff.Configuration;
using Microsoft.Extensions.Options;
using ComplianceSheriff.Requests;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    [Authorize]
    public class CheckpointGroupController : Controller
    {
        [HttpGet("checkpointGroupList")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CheckpointGroupListItem>))]
        public async Task<IActionResult> GetCheckpointGroupList([FromServices] ICheckpointGroupsAccessor accessor,
                                                                [FromServices] JwtSignInHandler jwtHandler,
                                                                CancellationToken cancellationToken)
        {
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());
            var organizationId = RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["organizationId"].ToString();

            var result = await accessor.GetCheckpointGroupList(userGroupId, organizationId, cancellationToken);
         
            return Ok(result);
        }

        [HttpGet("checkpointGroupsBy")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CheckpointGroupListItem>))]
        public async Task<IActionResult> GetCheckpointGroupBy([FromQuery] GetCheckpointGroupsByRequest request, [FromServices] ICheckpointGroupsAccessor accessor, [FromServices] JwtSignInHandler jwtHandler, CancellationToken cancellationToken)
        {
            var userGroupId = Convert.ToInt32(RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["userGroupId"].ToString());
            var organizationId = RestAPIUtils.GetJwtPayload(HttpContext, jwtHandler)["organizationId"].ToString();
            var result = await accessor.GetCheckpointGroupsBy(userGroupId, organizationId, request.ScanId, request.ScanGroupId, request.CheckpointGroupId, cancellationToken);

            return Ok(result);
        }

        [HttpGet("checkpointGroupings")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CheckpointGroupings>))]
        public async Task<IActionResult> GetCheckpointGroupings([FromServices] ICheckpointGroupService checkpointGroupService,
                                              [FromServices] ICheckpointGroupsAccessor accessor,
                                              CancellationToken cancellationToken)
        {
            var result = await accessor.GetCheckpointGroupings(cancellationToken);

            var scanResultsView = new ScanResults.ScanResultsView();

            var dictionary = new HybridDictionary();

            foreach (var group in result.CheckpointGroupScanAssociations.GroupBy(a => a.ScanId))
            {
                var groupIds = new StringCollection();
                groupIds.AddRange(group.Select(a => a.CheckpointGroupId).ToArray());
                dictionary[group.Key] = checkpointGroupService.GetPermittedCheckpointGroups(groupIds, result);
            };

            scanResultsView.PermittedCheckpointGroupsByScan = Newtonsoft.Json.JsonConvert.SerializeObject(dictionary);

            dictionary.Clear();

            foreach (var id in result.ScanGroups.Select(sg => sg.ScanGroupId).ToList().ConvertAll<string>(x => x.ToString()).ToArray())
            {
                var scanGroup = result.ScanGroups.Where(sg => sg.ScanGroupId.ToString() == id).FirstOrDefault();
                dictionary[id] = checkpointGroupService.GetPermittedCheckpointGroups(scanGroup, result);
            }

            scanResultsView.PermittedCheckpointGroupsByScanGroup = Newtonsoft.Json.JsonConvert.SerializeObject(dictionary);
            scanResultsView.CheckpointGroupDescriptionDictionary = Newtonsoft.Json.JsonConvert.SerializeObject(result.CheckpointGroupDescriptionDictionary);

            return Ok(scanResultsView);
        }
    }
}
