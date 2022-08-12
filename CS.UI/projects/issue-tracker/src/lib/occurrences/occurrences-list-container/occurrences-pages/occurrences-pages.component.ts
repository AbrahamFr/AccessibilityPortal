import { Component, Input, OnChanges, OnInit } from "@angular/core";
import { InteractionsService } from "interactions";
import { UrlParamsService } from "navigation";
import { IssueTrackerParamService } from "../../../issue-tracker-param.service";
import { OccurrencePage } from "../../../types";
import { OccurrencesParamService } from "../../occurrences-params.service";
import { OccurrencesService } from "../../occurrences.service";

@Component({
  selector: "app-occurrences-pages",
  templateUrl: "./occurrences-pages.component.html",
  styleUrls: ["./occurrences-pages.component.scss"],
})
export class OccurrencesPagesComponent implements OnInit, OnChanges {
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

  ngOnChanges() {}

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
