using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComplianceSheriff.TimeZones
{
    public class TimeZoneService : ITimeZoneService
    {
		private readonly ILogger<TimeZoneService> _logger;

		public TimeZoneService(ILogger<TimeZoneService> logger)
		{
			_logger = logger;
		}
		public async Task<List<TimeZoneListItem>> GetTimeZones()
		{
			try
			{
				var timeZoneList = new List<TimeZoneListItem>();

				var localTimeZones = TimeZoneInfo.GetSystemTimeZones();

				foreach (TimeZoneInfo tzi in localTimeZones)
				{
					timeZoneList.Add(new TimeZoneListItem { Id = tzi.Id, DisplayName = tzi.DisplayName });
				}

				await Task.Delay(1000);

				return timeZoneList.OrderBy(o => o.DisplayName).ToList();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message + " " + ex.StackTrace);
				throw;
			}
		}
	}
}
