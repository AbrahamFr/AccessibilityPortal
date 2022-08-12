import { Component, OnInit, Input, OnDestroy } from "@angular/core";
import { PRIMARY_OUTLET, Router } from "@angular/router";
import { IssueTrackerParamService } from "../../issue-tracker-param.service";
import { IssueTrackerParams, IssueTrackerSummary } from "cs-core";
import { OccurrencesParamService } from "../occurrences-params.service";
import { OrganizationIdService } from "navigation";
import { InteractionsService } from "interactions";

@Component({
  selector: "app-occurrences-filter-summary",
  templateUrl: "./occurrences-filter-summary.component.html",
  styleUrls: ["./occurrences-filter-summary.component.scss"],
})
export class OccurrencesFilterSummaryComponent implements OnInit, OnDestroy {
  constructor(
    private issueTrackerParamService: IssueTrackerParamService,
    private occurrencesParamService: OccurrencesParamService,
    private router: Router,
    private organizationIdService: OrganizationIdService,
    private interactionsService: InteractionsService
  ) {
    this.issueTrackerParams = this.issueTrackerParamService.getIssueTracker();
  }

  issueTrackerUrl = !this.organizationIdService.useOrgVirtualDir
    ? this.router.parseUrl(this.router.url).root.children[PRIMARY_OUTLET]
        .segments[0]
    : `${this.organizationIdService.orgVirtualDir}/Reports/IssueTracker`;
  issueTrackerParams: IssueTrackerParams;
  summaryFilters: IssueTrackerSummary | undefined;
  filterCount: number | null = null;
  scanDisplay: string = "";
  groupDisplay: string = "";
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnInit() {
    this.filterCount = this.setSearchFilterCount();
    this.scanDisplay = this.setScanDisplay();
    this.groupDisplay = this.useCInvStyles ? "Guideline" : "Checkpoint Group";
    this.summaryFilters = this.issueTrackerParamService.getIssueTrackerSummaryFilters();
  }

  ngOnDestroy(): void {
    const occurrencesParams = this.issueTrackerParamService.getOccurrences();
    if (occurrencesParams.recordsToReturn != 20) {
      occurrencesParams.recordsToReturn = 20;
      const appParamsObj = this.occurrencesParamService.updateOccurrenceIssueTrackerAppParamsObj(
        occurrencesParams
      );
      this.issueTrackerParamService.updateIssueTrackerUrlDataParams(
        appParamsObj
      );
    }
  }

  setScanDisplay(): string {
    return this.useCInvStyles
      ? "Audit"
      : this.issueTrackerParams.hasOwnProperty("scanId")
      ? "Scan"
      : "Scan Group";
  }

  setSearchFilterCount(): number | null {
    this.summaryFilters = this.issueTrackerParamService.getIssueTrackerSummaryFilters();
    const searchFilters =
      this.summaryFilters && this.summaryFilters.issueTrackerSearchFilters;
    return searchFilters && searchFilters.length ? searchFilters.length : null;
  }

  onReturnToIssuesClick() {
    this.router.navigate([`${this.issueTrackerUrl}`], {
      queryParams: this.issueTrackerParamService.issueTrackerUrlDataParams$
        .value,
    });
  }
}
