import * as moment from "moment";
import { DateRangeType } from "../app/data-types/DateOption";
import { neverEver } from "cs-core";

export function getStartDate(value: string | null): string {
  if (value === null) {
    return "";
  }
  return moment().subtract(parseInt(value), "days").format("L");
}

export function getEndDate(): string {
  return moment().format("L");
}

export function getStartDateFromRange(
  dateRange: DateRangeType,
  endDate: string
) {
  const endDateMoment = moment(endDate);
  switch (dateRange) {
    case "Year":
      return endDateMoment.add("year", -1).format("L");
    case "Week":
      return endDateMoment.add("day", -7).format("L");
    case "Month":
      return endDateMoment.add("month", -1).format("L");
    case "Day":
      return endDateMoment.add("day", -1).format("L");
    case "3 Months":
      return endDateMoment.add("month", -3).format("L");
    case "Custom":
      throw new Error("Cannot calculate a start date on a custom range.");
    default:
      return neverEver(dateRange);
  }
}

export function dateRangeDayCount(startDate: string, endDate: string): number {
  return moment(endDate).diff(startDate, "days");
}
