import { Component, Input, OnChanges } from "@angular/core";
import { InteractionsService } from 'interactions';
import { IssueTrackerIssues } from "../types";
import { IssueTrackerService } from "../issue-tracker.service";
import { IssueTrackerParamService } from "../issue-tracker-param.service";

@Component({
  selector: "app-issue-tracker-summary",
  templateUrl: "./issue-tracker-summary.component.html",
  styleUrls: ["./issue-tracker-summary.component.scss"],
})
export class IssueTrackerSummaryComponent implements OnChanges {
  @Input()
  issueTrackerIssues: IssueTrackerIssues;

  constructor(
    private issueTrackerParamService: IssueTrackerParamService,
    private issueTrackerService: IssueTrackerService,
    private interactionsService: InteractionsService
  ) {
    this.issueTrackerParamService.setActiveQuickFilter();
  }

  hasScansOrScanGroups$ = this.issueTrackerService.hasScansOrScanGroups$;
  activeQuickFilter$ = this.issueTrackerParamService.activeQuickFilter$;
  issues: IssueTrackerIssues;
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    if (
      this.issueTrackerService.refreshIssueTrackerSummaryView(
        this.issueTrackerIssues
      )
    ) {
      this.issues = this.issueTrackerIssues;
    }
  }

  onToggelQuickFilter(evt: KeyboardEvent | any) {
    const filter = evt.srcElement.id;
    this.issueTrackerParamService.toggleQuickFilter(filter);
    this.issueTrackerService.resetRefreshIssueTrackerSummaryView();
  }
}
