import { Frequency } from "./DateOption";

export class CheckpointFailures {
  checkpointId: string;
  description: string;
  currentFailures: number;
  oneRunBackFailures: number;
  twoRunsBackFailures: number;
  priority1Failures: number;
  pagesImpacted: number;
}

export class PageFailures {
  pageUrl: string;
  currentCheckPointFailures: number;
  oneRunBackCheckpointFailures: number;
  twoRunsBackCheckpointFailures: number;
  priority1Failures: number;
  priority2Failures: number;
  priority3Failures: number;
}

export class ScanGroupMetrics {
  displayName: string;
  scanGroupId: number;
  scanMetrics: ScanMetrics[];
}

export class ScanMetrics {
  scanType: string;
  scanResult: number;
  metrics: {
    changeDirection: string;
    percentChange: number;
    flagWarning: boolean;
  };
}

export type ScanGroupPerformanceMetrics = {
  scanGroupId: number;
  performanceType: string;
  metrics: PerformanceMetrics;
};

export type PerformanceMetrics = {
  scanTotal: number;
  passedTotal: number;
  passedPercent: number;
  failedTotal: number;
  failedPercent: number;
};

export class ScanChartData {
  frequency: Frequency;
  metrics: ScanMetric[];
}

export type ScanHistoryData = {
  failedCheckpointPercent: number;
  failedCheckpoints: number;
  failedPagePercent: number;
  failedPages: number;
  passedCheckpointPercent: number;
  passedCheckpoints: number;
  passedPagePercent: number;
  passedPages: number;
  runDate: string;
  scanGroupId: number;
  scanGroupRunId: number;
  totalCheckpoints: number;
  totalPages: number;
};

export type ChartPerformanceOption = {
  performanceType: string;
  displayName: string;
  settings: any;
};

export class ScanMetric {
  period: string;
  scans: number;
  failedPages: number;
}

export class ChartData {
  x: number;
  y: number;
}

export class RangeOption {
  display: string;
  frequency: Frequency;
}

export interface SearchQueryParams {
  scanGroupId: number;
}

export type AuditReport = {
  auditReportId: number;
  reportName: string;
  auditTypeId: number;
  fileUploadDate: string;
  fileType: string;
  fileSize: number;
  reportDescription: string;
  fileLocation: string;
  auditTypeName: string;
  fileStatusId: number;
  fileStatusName: string;
  canEdit: boolean;
};

export type AuditReportFormData = {
  reportName: string;
  reportType: string;
  reportDescription: string;
};
