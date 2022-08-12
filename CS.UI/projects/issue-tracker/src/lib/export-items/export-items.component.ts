import { Component, Input, AfterContentChecked } from "@angular/core";
import { AuthenticationService } from "authentication";
import { InteractionsService } from "interactions";
import { IssueTrackerService } from "../issue-tracker.service";
import { OccurrencesService } from "../occurrences/occurrences.service";

@Component({
  selector: "app-export-items",
  templateUrl: "./export-items.component.html",
  styleUrls: ["./export-items.component.scss"],
})
export class ExportItemsComponent implements AfterContentChecked {
  @Input()
  orgVirtualDir: string | null;
  @Input()
  childRoute: string;

  hasIssueTrackerPerm: boolean;
  hasNoData: boolean = true;

  issueTrackerOccurrencesHasNoData() {
    if (
      this.childRoute &&
      (this.childRoute.toLowerCase().endsWith("issuetracker") ||
        this.childRoute.toLocaleLowerCase().endsWith("auditresults"))
    ) {
      this.issueTrackerService.issueTrackerHasNoData$.subscribe((x) => {
        this.hasNoData = x;
      });
    } else {
      this.occurrencesService.occurrencesHasNoData$.subscribe((x) => {
        this.hasNoData = x;
      });
    }
  }

  constructor(
    private authService: AuthenticationService,
    private issueTrackerService: IssueTrackerService,
    private occurrencesService: OccurrencesService,
    private interactionsService: InteractionsService
  ) {}

  useCInvStyles = this.interactionsService.useCInvStyles;

  ngAfterContentChecked(): void {
    this.hasIssueTrackerPerm = this.authService.permissionCheck("IssueTracker");
    this.issueTrackerOccurrencesHasNoData();
  }

  onExportClick() {
    switch (this.childRoute) {
      case "Reports/IssueTracker": {
        this.issueTrackerService.exportActive$.next(true);
        break;
      }
      case "auditresults": {
        this.issueTrackerService.exportActive$.next(true);
        break;
      }
      case "Reports/IssueTracker/Occurrences": {
        this.occurrencesService.occurrencesExportActive$.next(true);
        break;
      }
      case "auditresults/Occurrences": {
        this.occurrencesService.occurrencesExportActive$.next(true);
        break;
      }
    }
  }
}
