import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpResponse,
} from "@angular/common/http";
import {
  map,
  publishReplay,
  startWith,
  refCount,
  catchError,
  tap,
} from "rxjs/operators";
import { Observable, of, throwError, BehaviorSubject } from "rxjs";
import { CheckpointItem, UrlParams } from "cs-core";
import {
  catchObservableError,
  ObservableError,
  makeObservableError,
} from "api-handler";
import { IssueTrackerIssues } from "./types";

@Injectable({
  providedIn: "root",
})
export class IssueTrackerService {
  private issueTrackerListUrl: string = "rest/IssueTracker/IssueTrackerList";
  private issueTrackerListExportUrl: string =
    "rest/IssueTracker/IssueTrackerExport";

  readonly checkpointsList$ = new BehaviorSubject<CheckpointItem[]>([]);
  readonly exportActive$ = new BehaviorSubject<boolean>(false);
  readonly issueTrackerHasNoData$ = new BehaviorSubject<boolean>(true);
  readonly hasScansOrScanGroups$ = new BehaviorSubject<boolean>(false);
  private readonly refreshIssueTrackerSummaryView$ = new BehaviorSubject<
    boolean
  >(true);

  constructor(private http: HttpClient) {}

  filterOnHasScanOrScanGroup(data) {
    let hasData = data[0].some((p) => p);
    if (hasData) {
      this.hasScansOrScanGroups$.next(true);
      return true;
    }
    this.hasScansOrScanGroups$.next(false);
    return false;
  }

  getIssueTrackerList(
    urlParams: UrlParams
  ): Observable<IssueTrackerIssues | ObservableError> {
    return this.http.post(this.issueTrackerListUrl, urlParams).pipe(
      startWith(0),
      map((response) => response as IssueTrackerIssues),
      catchError((response) => {
        if (response instanceof HttpErrorResponse && response.status === 424) {
          return of(
            makeObservableError(
              response,
              "api:issueTracker:issueTrackerList:noLicenseKeyFound"
            )
          );
        } else {
          return throwError(response);
        }
      }),
      catchObservableError("api:issueTracker:apiError"),
      publishReplay(1),
      refCount(),
      tap((issues) => this.checkpointsList$.next(issues["checkpointList"]))
    );
  }

  exportIssueTrackerReport(
    urlParams: UrlParams
  ): Observable<HttpResponse<any> | ObservableError> {
    return this.http
      .post(this.issueTrackerListExportUrl, urlParams, {
        responseType: "blob",
        observe: "response",
      })
      .pipe(
        map((response) => response as HttpResponse<any>),
        catchObservableError("api:issueTracker:apiError")
      );
  }
  setIssueTrackerHasNoData(issues: IssueTrackerIssues) {
    if (issues && issues.totalIssuesFound > 0) {
      this.issueTrackerHasNoData$.next(false);
    } else {
      this.issueTrackerHasNoData$.next(true);
    }
  }

  resetRefreshIssueTrackerSummaryView() {
    this.refreshIssueTrackerSummaryView$.next(false);
  }
  refreshIssueTrackerSummaryView(issueTrackerIssues: IssueTrackerIssues) {
    const refreshSummary = this.refreshIssueTrackerSummaryView$.value;
    if (!refreshSummary && issueTrackerIssues) {
      this.refreshIssueTrackerSummaryView$.next(true);
    }
    return refreshSummary;
  }
}
