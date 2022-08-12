import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ScanChartData, ScanHistoryData } from "../../../data-types/types";
import { catchObservableError, ObservableError } from "api-handler";

@Injectable({ providedIn: "root" })
export class ScanChartService {
  private scanChartUrl = "rest/Trend/scangroupscanhistory";
  constructor(private http: HttpClient) {}

  public scanChartResultsList(
    scanGroupId: string
  ): Observable<ScanHistoryData[] | ObservableError> {
    return this.http
      .post<ScanHistoryData[]>(this.scanChartUrl, {
        scanGroupId,
      })
      .pipe(catchObservableError("api:trend:apiError"));
  }
}
