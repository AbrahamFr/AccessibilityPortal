import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ScanGroupPerformanceMetrics } from "../../../data-types/types";
import { ObservableError, catchObservableError } from "api-handler";

@Injectable({ providedIn: "root" })
export class PagePerformanceResultsService {
  private pagePerformanceResultsUrl =
    "rest/Trend/scangrouppageperformancemetrics";
  constructor(private http: HttpClient) {}

  public pagePerformanceResultsList(
    scanGroupId: string
  ): Observable<ScanGroupPerformanceMetrics | ObservableError> {
    return this.http
      .post<ScanGroupPerformanceMetrics>(this.pagePerformanceResultsUrl, {
        scanGroupId,
      })
      .pipe(catchObservableError("api:trend:apiError"));
  }
}
