using System;
using System.Collections.Generic;
using System.Text;

namespace ComplianceSheriff
{
    public class DateRange
    {
        public DateRange(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

        public DateTimeOffset? StartDate { get; }
        public DateTimeOffset? EndDate { get; }

        public static DateRange From(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            return new DateRange(startDate, endDate);
        }
    }
}
