using ComplianceSheriff.TimeZones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComplianceSheriff.Controllers
{
    [Route("rest/[controller]")]
    public class TimeZoneController : Controller
    {
        private readonly ITimeZoneService _timeZoneService;

        public TimeZoneController(ITimeZoneService timeZoneService)
        {
            _timeZoneService = timeZoneService;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(List<TimeZoneListItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTimeZones()
        {
            var timeZones = await _timeZoneService.GetTimeZones();
            return Ok(timeZones);
        }
    }
}
