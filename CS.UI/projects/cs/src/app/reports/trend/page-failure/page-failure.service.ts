import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { PageFailures } from "../../../data-types/types";
import { catchObservableError, ObservableError } from "api-handler";

@Injectable({ providedIn: "root" })
export class PageFailureService {
  private pageFailureUrl = "rest/Trend/scangrouptop10pageFailures";

  constructor(private http: HttpClient) {}

  public pageFailureResultsList(
    scanGroupId: string
  ): Observable<PageFailures[] | ObservableError> {
    return this.http
      .post<PageFailures[]>(this.pageFailureUrl, {
        scanGroupId,
      })
      .pipe(catchObservableError("api:trend:apiError"));
  }
}
