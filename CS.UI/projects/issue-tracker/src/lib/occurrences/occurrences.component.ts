import { Component, OnDestroy, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { Title } from "@angular/platform-browser";

import { UrlParamsService } from "navigation";
import { InteractionsService } from "interactions";
// import { AppSettings } from "../../../constants/app-settings";
import { OccurrencesParamService } from "./occurrences-params.service";
import { IssueTrackerParamService } from "../issue-tracker-param.service";

@Component({
  selector: "app-occurrences",
  templateUrl: "./occurrences.component.html",
  styleUrls: ["./occurrences.component.scss"],
})
export class OccurrencesComponent implements OnDestroy, OnInit {
  constructor(
    private titleService: Title,
    private urlParamService: UrlParamsService,
    private router: Router,
    private occurrenceParamService: OccurrencesParamService,
    private issueTrackerParamService: IssueTrackerParamService,
    private interactionsService: InteractionsService
  ) {
    // this.titleService.setTitle(
    //   AppSettings.PAGETITLE + " (Reports - Issue Tracker Occurrences)"  // TODO: Solve this
    // );
    this.router.events.subscribe((event) =>
      this.urlParamService.openIsNavPopstate(event)
    );

    this.occurrenceParamService.checkInitialState();
    // Return scroll position to top of page when navigating to Occurrences Report ** IE11 window Object does not have 'scrollY' property **
    if (window.scrollY || window.screenY) {
      window.scroll(0, 0);
    }
  }
  useCInvStyles = this.interactionsService.useCInvStyles;
  checkpoint: string = "";

  ngOnInit() {
    const occurrencesParams = this.issueTrackerParamService.getOccurrences();
    this.checkpoint = occurrencesParams.checkpoint!;
  }
  ngOnDestroy() {
    this.resetOccurrenceSearchFilters();
  }

  resetOccurrenceSearchFilters() {
    let searchParamObj = {
      pageTitle: null,
      pageUrl: null,
      element: null,
      keyAttribute: null,
      containerId: null,
    };
    let summaryFilters = this.issueTrackerParamService.getOccurrencesSummaryFilters();
    summaryFilters = {
      ...summaryFilters,
      ...{ occurrenceSearchFilters: [] },
    };
    const summaryAndSearchFilters = {
      ...searchParamObj,
      ...{ summaryFilters },
    };
    const appParamsObj = this.occurrenceParamService.updateOccurrenceIssueTrackerAppParamsObj(
      summaryAndSearchFilters
    );
    this.issueTrackerParamService.updateIssueTrackerUrlDataParams(appParamsObj);
  }
}
