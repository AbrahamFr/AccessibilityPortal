import { Component, Input, OnChanges } from "@angular/core";
import { Router } from "@angular/router";
import { BehaviorSubject } from "rxjs";
import { IssueTrackerSummary, OccurrenceParams } from "cs-core";
import { InteractionsService } from "interactions";
import { UrlParamsService, OrganizationIdService } from "navigation";
import { IssueTrackerIssues } from "../types";
import { IssueTrackerService } from "../issue-tracker.service";
import { IssueTrackerParamService } from "../issue-tracker-param.service";
import { OccurrencesService } from "../occurrences/occurrences.service";

@Component({
  selector: "app-issue-tracker-list",
  templateUrl: "./issue-tracker-list.component.html",
  styleUrls: ["./issue-tracker-list.component.scss"],
})
export class IssueTrackerListComponent implements OnChanges {
  @Input()
  issues: IssueTrackerIssues;

  constructor(
    private issueTrackerService: IssueTrackerService,
    private router: Router,
    private issueTrackerParamService: IssueTrackerParamService,
    private occurrenceService: OccurrencesService,
    private urlParamService: UrlParamsService,
    private organizationIdService: OrganizationIdService,
    private interactionsService: InteractionsService
  ) {}

  issueTrackerOccurrencesUrl = !this.organizationIdService.useOrgVirtualDir
    ? this.router.url.split("?")[0] + "/Occurrences"
    : `${this.organizationIdService.orgVirtualDir}/Reports/IssueTracker/Occurrences`;
  readonly activeFilter$ = new BehaviorSubject<boolean>(false);
  readonly activeSort$ = new BehaviorSubject<boolean>(false);

  filteredIssuesCount: number;
  hasScansOrScanGroups$ = this.issueTrackerService.hasScansOrScanGroups$;
  issueTrackerSearchFilters: string[] | undefined = [];
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    const summaryFilters = this.issueTrackerParamService.getIssueTrackerSummaryFilters();
    this.issueTrackerSearchFilters =
      summaryFilters && summaryFilters.issueTrackerSearchFilters;
    this.issueTrackerService.setIssueTrackerHasNoData(this.issues);
    this.filteredIssuesCount = this.issues && this.issues.totalFilteredRecords;
    this.setFocusOnNavFromOccurrences();
  }

  displayedColumns$ = new BehaviorSubject<string[]>([
    "issue",
    "severity",
    "impact",
    "occurrences",
    "pages",
    "highestPageLevel",
    "priorityLevel",
    "checkpoint",
    "state",
  ]);

  setFocusOnNavFromOccurrences() {
    const previousUrlPath = this.urlParamService.getPreviousUrlPath();
    if (previousUrlPath && previousUrlPath.includes("Occurrences")) {
      const summaryFilters = this.issueTrackerParamService.getOccurrencesSummaryFilters();
      const activeElId =
        summaryFilters && summaryFilters.activatedIssueTrackerElementId;
      if (activeElId) {
        setTimeout(() => {
          const element = document.getElementById(activeElId);
          if (element) {
            element.focus();
          }
        });
      }
    }
  }

  onFilterClick() {
    this.activeFilter$.next(!this.activeFilter$.value);
    this.activeSort$.next(false);
  }

  onSortClick() {
    this.activeSort$.next(!this.activeSort$.value);
    this.activeFilter$.next(false);
  }

  onCloseFilter() {
    this.activeFilter$.next(!this.activeFilter$.value);
    this.setFilterButtonFocus();
  }

  onCloseSort() {
    this.activeSort$.next(!this.activeSort$.value);
    this.setSortButtonFocus();
  }

  setFilterButtonFocus() {
    const filterBtn = document.getElementById(
      "issue-tracker-filter-button"
    ) as HTMLInputElement;
    if (filterBtn) {
      setTimeout(() => {
        filterBtn.focus();
      }, 0);
    }
  }

  setSortButtonFocus() {
    const sortBtn = document.getElementById(
      "issue-tracker-sort-button"
    ) as HTMLInputElement;
    if (sortBtn) {
      setTimeout(() => {
        sortBtn.focus();
      }, 0);
    }
  }

  getMoreData(pageination: object) {
    let recordsToReturn = this.issueTrackerParamService.getIssueTrackerRecordsToReturn();
    if (recordsToReturn) {
      recordsToReturn += pageination["numberOfRecordsToFetchMore"];
    }
    this.issueTrackerParamService.updateUrlParmsAndRefresh({ recordsToReturn });
  }

  onOccurrencesClick(
    params: Partial<OccurrenceParams>,
    occurrenceSummaryParams: Partial<IssueTrackerSummary>,
    context: string
  ) {
    this.occurrenceService.updateActiveOccurrencesListTab(context);
    const updatedUrlDataParams = this.issueTrackerParamService.navigateToOccurrences(
      params,
      occurrenceSummaryParams
    );
    this.router.navigate([`${this.issueTrackerOccurrencesUrl}`], {
      queryParams: updatedUrlDataParams,
    });
  }

  noDataMessage() {
    const result = this.issues.totalPagesScanned > 0 && this.issues.totalPagesImpacted == 0;
    return result == true ? "No issues found" : "No data available"
  }
}
