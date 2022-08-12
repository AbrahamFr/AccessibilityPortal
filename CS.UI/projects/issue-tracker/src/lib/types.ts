import { CheckpointItem, KeyValueString } from "cs-core";

export type IssueTrackerItem = {
  issue: string;
  issueId: number;
  severity: string;
  impact: string;
  priorityLevel: string;
  highestPageLevel: number;
  occurrences: number;
  pages: number;
  scans: number;
  checkpointSubGroups: string;
  checkpoint: string;
  checkpointId: string;
  state: string;
};

export type IssueTrackerIssues = {
  checkpointList: CheckpointItem[];
  issueTrackerList: IssueTrackerItem[];
  totalIssuesFound: number;
  totalFilteredRecords: number;
  totalFailedIssues: number;
  totalHighSeverityIssues: number;
  totalOccurrences: number;
  totalPagesImpacted: number;
  totalPagesScanned: number;
};

export type Occurrence = {
  pageTitle?: string;
  pageUrl?: string;
  lineNumber?: number;
  columnNumber?: number;
  keyAttribute?: string;
  scanDisplayName?: string;
  state?: string;
  resultId?: number;
  cachedPageLink?: string;
  cachedPageUrl?: string;
  scanId?: number;
  scanGroupId?: number;
  containerId?: string;
  element?: string;
};

export type OccurrenceList = {
  occurrencesList?: Occurrence[];
  titleFilterList?: KeyValueString[];
  urlFilterList?: string[];
  keyAttributeFilterList?: KeyValueString[];
  elementFilterList?: KeyValueString[];
  containerIdFilterList?: KeyValueString[];
  totalOccurrences: number;
  totalFilteredRecords: number;
};

export type OccurrencePage = {
  occurrencePages: PageOccurrence[];
  totalPages: number;
  totalFilteredPages: number;
};

export type PageOccurrence = {
  pageTitle: string;
  pageUrl: string;
  noOfOccurrences: number;
  cachedPageLink: string;
  occurrences: Occurrence[];
};

export type OccurrenceSummary = {
  issue: string | undefined;
  totalOccurrences: number | undefined;
  totalPages: number | undefined;
  priorityLevel: string | undefined;
  state: string | undefined;
};
