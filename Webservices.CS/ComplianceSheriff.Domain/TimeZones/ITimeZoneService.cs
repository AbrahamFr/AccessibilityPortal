using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComplianceSheriff.TimeZones
{
    public interface ITimeZoneService
    {
        Task<List<TimeZoneListItem>> GetTimeZones();
    }
}
