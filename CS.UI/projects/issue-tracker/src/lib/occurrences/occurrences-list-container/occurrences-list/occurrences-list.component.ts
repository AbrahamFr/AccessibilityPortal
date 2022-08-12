import { Component, Input, OnInit, OnChanges } from "@angular/core";
import { InteractionsService } from "interactions";
import { UrlParamsService } from "navigation";
import { BehaviorSubject } from "rxjs";
import { IssueTrackerParamService } from "../../../issue-tracker-param.service";
import { Occurrence, OccurrenceList, OccurrencePage } from "../../../types";
import { OccurrencesParamService } from "../../occurrences-params.service";
import { OccurrencesService } from "../../occurrences.service";

@Component({
  selector: "app-occurrences-list",
  templateUrl: "./occurrences-list.component.html",
  styleUrls: ["./occurrences-list.component.scss"],
})
export class OccurrencesListComponent implements OnInit, OnChanges {
  @Input()
  occurrences: OccurrenceList;
  @Input()
  occurrencesByPage: OccurrencePage;

  constructor(
    private urlParamService: UrlParamsService,
    private occurrenceService: OccurrencesService,
    private occurrencesParamService: OccurrencesParamService,
    private issueTrackerParamService: IssueTrackerParamService,
    private interactionsService: InteractionsService
  ) {}

  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnInit() {}

  ngOnChanges() {
    if (
      this.occurrences &&
      this.occurrences.occurrencesList &&
      this.occurrences.occurrencesList.length > 0
    ) {
      this.occurrenceService.occurrencesHasNoData$.next(false);
    } else {
      if (
        this.occurrences &&
        this.occurrencesByPage &&
        this.occurrencesByPage.occurrencePages &&
        this.occurrencesByPage.occurrencePages.length > 0
      ) {
        // Facade: present a page as an occurrence to the user when a visual check is needed of the whole page.
        this.occurrences.occurrencesList = [];
        this.occurrencesByPage.occurrencePages.forEach((page) => {
          const occurrence: Occurrence = {
            element: "--",
            keyAttribute: "--",
            containerId: "--",
            pageTitle: page.pageTitle,
            pageUrl: page.pageUrl,
            cachedPageUrl: page.cachedPageLink,
          };
          this.occurrences.occurrencesList?.push(occurrence);
        });
        this.occurrenceService.occurrencesHasNoData$.next(false);
      } else {
        this.occurrenceService.occurrencesHasNoData$.next(true);
      }
    }
  }

  displayedColumns$ = new BehaviorSubject<string[]>([
    "element",
    "keyAttribute",
    "containerId",
    "pageTitle",
    "pageUrl",
  ]);

  getMoreData(pageination: object) {
    const issueTrackerAppParams = this.issueTrackerParamService
      .issueTrackerAppParams$.value;
    const occurrencesParams = issueTrackerAppParams.occurrence;
    let recordsToReturn = occurrencesParams.recordsToReturn;
    if (recordsToReturn) {
      recordsToReturn += pageination["numberOfRecordsToFetchMore"];
    }
    occurrencesParams.recordsToReturn = recordsToReturn;
    const appParamsObj = this.occurrencesParamService.updateOccurrenceIssueTrackerAppParamsObj(
      occurrencesParams
    );
    const updatedUrlDataParams = this.issueTrackerParamService.updateIssueTrackerUrlDataParams(
      appParamsObj
    );
    this.urlParamService.updateUrlParams(updatedUrlDataParams);
  }
}
