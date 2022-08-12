import { IssueTrackerParams, OccurrenceParams, TrendParams } from "cs-core";

export type UrlDataParams = {
  scanId?: string | number | undefined;
  scanGroupId?: string | number | undefined;
  data?: string;
};

export type AppParams = {
  issueTracker?: IssueTrackerParams;
  occurrence?: OccurrenceParams;
  trend?: TrendParams;
};
