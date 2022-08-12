import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ScanGroupPerformanceMetrics } from "../../../data-types/types";
import { ObservableError, catchObservableError } from "api-handler";

@Injectable({ providedIn: "root" })
export class CheckpointPerformanceResultsService {
  private checkpointPerformanceResultsUrl =
    "rest/Trend/scangroupcheckpointperformancemetrics";
  constructor(private http: HttpClient) {}

  public checkpointPerformanceResultsList(
    scanGroupId: string
  ): Observable<ScanGroupPerformanceMetrics | ObservableError> {
    return this.http
      .post<ScanGroupPerformanceMetrics>(this.checkpointPerformanceResultsUrl, {
        scanGroupId,
      })
      .pipe(catchObservableError("api:trend:apiError"));
  }
}
