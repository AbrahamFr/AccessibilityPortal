import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ScanGroupMetrics } from "../../../data-types/types";
import { catchObservableError, ObservableError } from "api-handler";

@Injectable({ providedIn: "root" })
export class ScanPerformanceService {
  private scanPerformanceUrl = "rest/Trend/scangroupscanperformance";
  constructor(private http: HttpClient) {}

  public scanPerformanceResultsList(
    scanGroupId: string
  ): Observable<ScanGroupMetrics[] | ObservableError> {
    return this.http
      .post<ScanGroupMetrics[]>(this.scanPerformanceUrl, {
        scanGroupId,
      })
      .pipe(catchObservableError("api:trend:apiError"));
  }
}
