export class ScanGroup {
  scanGroupId: string | number;
  displayName: string;
  lastScanDate: string;
}

export type CheckpointGroup = {
  checkpointGroupId: string;
  shortDescription: string;
};

export type CheckpointItem = {
  checkpointId: string;
  checkpointDescription: string;
};

export type ImpactRange = {
  minImpact: number;
  maxImpact: number;
};

export type BaseUrlParams = {
  currentPage?: number;
  recordsToReturn?: number;
  scanId?: string | number | undefined;
  scanGroupId?: string | number | undefined;
  checkpointGroupId?: string | number | undefined;
  checkpointId?: string | undefined;
  sortColumn?: string | undefined;
  sortDirection?: string | undefined;
  summaryFilters?: IssueTrackerSummary | undefined;
};

export interface IssueTrackerUrlParams extends BaseUrlParams {
  severity?: string[] | undefined;
  impactRange?: ImpactRange[] | undefined;
  priorityLevel?: number[] | undefined;
  state?: string[] | undefined;
  quickFilter?: string;
}

export interface OccurrenceUrlParams extends BaseUrlParams {
  issueId?: number;
  pageTitle?: string | undefined;
  pageUrl?: string | undefined;
  keyAttribute?: string | undefined;
  element?: string | undefined;
  containerId?: string | undefined;
  checkpoint?: string | undefined;
}

interface CreateUserUrlParams {
  firstName?: string | undefined;
  lastName?: string | undefined;
  emailAddress?: string | undefined;
  userName?: string | undefined;
  passWord?: string | undefined;
  organizationId: string | undefined;
  userGroupName: string | undefined;
}

interface CreateScanGroupUrlParams {
  scanGroupName: string;
  setAsDefault?: boolean;
}

interface UpdateScanGroupNameUrlParams {
  scanGroupId: number;
  scanGroupDisplayName: string;
}

interface UpdateUserGroupNameUrlParams {
  userGroupId: number;
  userGroupName: string;
}

interface AddScanGroupScansUrlParams {
  scanGroupId: number;
  scanList: number[];
}

export type UpdateScanGroupNameParams = UpdateScanGroupNameUrlParams;

export type UpdateUserGroupNameParams = UpdateUserGroupNameUrlParams;

export type AddScanGroupScansParams = AddScanGroupScansUrlParams;

export type CreateScanGroupParams = CreateScanGroupUrlParams;

export type CreateUserParams = CreateUserUrlParams;

export type IssueTrackerParams = IssueTrackerUrlParams;

export type OccurrenceParams = OccurrenceUrlParams;

export type UrlParams = IssueTrackerUrlParams & OccurrenceUrlParams;

export interface ExportReportParams extends UrlParams {
  exportFormat?: string;
  fileName?: string;
}

export type TrendParams = {
  scanGroupId?: string | undefined;
};

export type IssueTrackerAppParams = {
  issueTracker: IssueTrackerParams;
  occurrence: OccurrenceParams;
};

export type IssueTrackerSummary = {
  scanDisplayname?: string;
  checkpointGroupDisplayname?: string;
  issueTrackerSearchFilters?: string[];
  issue?: string;
  totalOccurrences?: number;
  totalPages?: number;
  priorityLevel?: string;
  state?: string;
  occurrenceSearchFilters?: string[];
  activatedIssueTrackerElementId?: string;
};

export type KeyValueString = {
  key: string;
  value: string;
};
