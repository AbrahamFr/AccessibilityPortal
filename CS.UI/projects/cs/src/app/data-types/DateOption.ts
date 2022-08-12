export type DateRangeType =
  | "Year"
  | "Week"
  | "Month"
  | "Day"
  | "3 Months"
  | "Custom";

export type Frequency = "YEAR" | "MONTH" | "WEEK" | "DAY";

export type DateFixedRange = {
  startDate: string;
  endDate: string;
};

export type DateDynamicRange = {
  rangeType: DateRangeType;
};

export type DateOptionRange = DateFixedRange | DateDynamicRange;

export interface DateOption {
  frequency: Frequency;
  dateRange: DateOptionRange;
}

export const isDynamicRange = (
  dateOption: DateOptionRange
): dateOption is DateDynamicRange => {
  return !!(dateOption as any).rangeType;
};
