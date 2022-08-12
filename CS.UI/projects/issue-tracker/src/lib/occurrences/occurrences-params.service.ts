import { Injectable } from "@angular/core";
import { UrlParamsService, OrganizationIdService } from "navigation";
import { BehaviorSubject } from "rxjs";
import {
  OccurrenceParams,
  IssueTrackerParams,
  IssueTrackerAppParams,
} from "cs-core";
import { IssueTrackerParamService } from "../issue-tracker-param.service";
import { Router, ActivatedRoute } from "@angular/router";

@Injectable({
  providedIn: "root",
})
export class OccurrencesParamService {
  readonly occurrencesParams$ = new BehaviorSubject<OccurrenceParams>({
    currentPage: 1,
    recordsToReturn: 20,
  });

  constructor(
    private urlParamService: UrlParamsService,
    private issueTrackerParamService: IssueTrackerParamService,
    private router: Router,
    private route: ActivatedRoute,
    private organizationIdService: OrganizationIdService
  ) {}

  issueTrackerUrl = !this.organizationIdService.useOrgVirtualDir
    ? this.router.url.split("?")[0]
    : `${this.organizationIdService.orgVirtualDir}/Reports/IssueTracker`;

  checkInitialState() {
    const queryParams = this.route.snapshot.queryParams;
    const previousUrl = this.urlParamService.getIsNavPopstate();
    const isNavPopstate = this.urlParamService.getIsNavPopstate();

    if (Object.keys(queryParams).length > 0 && !previousUrl) {
      if (
        queryParams["data"] &&
        (queryParams["scanId"] || queryParams["scanGroupId"])
      ) {
        const decodedSegmentData = this.urlParamService.decodeHash(
          queryParams.data
        );
        if (decodedSegmentData) {
          //
          // TODO - cleanse bookmarked params before saving
          //
          this.issueTrackerParamService.setIssueTrackerAppParam(
            decodedSegmentData
          );
          this.issueTrackerParamService.setIssueTrackerUrlDataParams(
            queryParams
          );
          this.urlParamService.updateUrlParams(queryParams);
        } else {
          this.router.navigate([`${this.issueTrackerUrl}`]);
        }
      } else {
        this.router.navigate([`${this.issueTrackerUrl}`]);
      }
    } else if (isNavPopstate) {
      this.urlParamService.updateUrlParams(
        this.issueTrackerParamService.getIssueTrackerUrlDataParams()
      );
      this.urlParamService.closeIsNavePopstate();
    } else if (Object.keys(queryParams).length == 0) {
      this.router.navigate([`${this.issueTrackerUrl}`]);
    } else {
      this.urlParamService.updateUrlParams(
        this.issueTrackerParamService.getIssueTrackerUrlDataParams()
      );
    }
  }

  updateOccurrenceIssueTrackerAppParamsObj(
    params: IssueTrackerParams
  ): IssueTrackerAppParams {
    const issueTrackerParams = this.issueTrackerParamService.getIssueTracker();
    const occurrenceParams = this.issueTrackerParamService.getOccurrences();
    const updatedOccurrenceParams = Object.assign(occurrenceParams, params);
    const issueTrackerAppParams: IssueTrackerAppParams = {
      ...{ issueTracker: issueTrackerParams },
      ...{ occurrence: updatedOccurrenceParams },
    };
    this.issueTrackerParamService.setIssueTrackerAppParam(
      issueTrackerAppParams
    );
    return issueTrackerAppParams;
  }

  clearRecordsToReturn() {
    const occurrencesParams = this.issueTrackerParamService.getOccurrences();
    if (occurrencesParams.recordsToReturn != 20) {
      occurrencesParams.recordsToReturn = 20;
      const appParamsObj = this.updateOccurrenceIssueTrackerAppParamsObj(
        occurrencesParams
      );
      const updatedUrlDataParams = this.issueTrackerParamService.updateIssueTrackerUrlDataParams(
        appParamsObj
      );
      this.urlParamService.updateUrlParams(updatedUrlDataParams);
    }
  }
}
