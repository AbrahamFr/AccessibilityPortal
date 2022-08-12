import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpResponse,
} from "@angular/common/http";
import { Observable, of, throwError, BehaviorSubject } from "rxjs";
import {
  map,
  publishReplay,
  startWith,
  refCount,
  catchError,
} from "rxjs/operators";
import { OccurrenceParams } from "cs-core";
import {
  catchObservableError,
  ObservableError,
  makeObservableError,
} from "api-handler";
import { OccurrenceList, PageOccurrence } from "../types";

@Injectable({
  providedIn: "root",
})
export class OccurrencesService {
  private occurrencesListUrl: string = "rest/IssueTracker/OccurrencesList";
  private occurrencesByPageUrl: string = "rest/IssueTracker/OccurrencesByPage";
  private occurrencesListExportUrl: string =
    "rest/IssueTracker/OccurrencesExport";

  readonly occurrencesActive$ = new BehaviorSubject<boolean>(true);
  readonly occurrencesExportActive$ = new BehaviorSubject<boolean>(false);
  readonly occurrencesHasNoData$ = new BehaviorSubject<boolean>(true);
  readonly activeOccurrencesListTab$ = new BehaviorSubject<string>(
    "occurrences"
  );

  constructor(private http: HttpClient) {}

  getOccurrencesList(
    occurrenceParams: OccurrenceParams
  ): Observable<OccurrenceList | ObservableError> {
    return this.http.post(this.occurrencesListUrl, occurrenceParams).pipe(
      startWith(0),
      map((response) => response as OccurrenceList),
      catchError((response) => {
        if (response instanceof HttpErrorResponse && response.status === 424) {
          return of(
            makeObservableError(
              response,
              "api:issueTracker:OccurrencesList:apiError"
            )
          );
        } else {
          return throwError(response);
        }
      }),
      catchObservableError("api:issueTracker:OccurrencesList:apiError"),
      publishReplay(1),
      refCount()
    );
  }

  getOccurrencesByPage(
    occurrenceParams: OccurrenceParams
  ): Observable<PageOccurrence | ObservableError> {
    return this.http.post(this.occurrencesByPageUrl, occurrenceParams).pipe(
      startWith(0),
      map((response) => response as PageOccurrence),
      catchError((response) => {
        if (response instanceof HttpErrorResponse && response.status === 424) {
          return of(
            makeObservableError(
              response,
              "api:issueTracker:OccurrencesByPage:apiError"
            )
          );
        } else {
          return throwError(response);
        }
      }),
      catchObservableError("api:issueTracker:OccurrencesByPage:apiError"),
      publishReplay(1),
      refCount()
    );
  }

  exportOccurrencesReport(
    occurrenceParams: OccurrenceParams
  ): Observable<HttpResponse<any> | ObservableError> {
    return this.http
      .post(this.occurrencesListExportUrl, occurrenceParams, {
        responseType: "blob",
        observe: "response",
      })
      .pipe(
        map((response) => response as HttpResponse<any>),
        catchObservableError("api:issueTracker:apiError")
      );
  }

  updateActiveOccurrencesListTab(context: string) {
    this.activeOccurrencesListTab$.next(context);
  }

  getActiveOccurrencesListTab() {
    return this.activeOccurrencesListTab$.value;
  }
  setActiveOccurrencesListTab(tab: string) {
    this.activeOccurrencesListTab$.next(tab);
  }
}
