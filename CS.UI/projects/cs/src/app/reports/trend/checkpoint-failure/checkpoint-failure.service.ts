import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { CheckpointFailures } from "../../../data-types/types";
import { ObservableError, catchObservableError } from "api-handler";

@Injectable({ providedIn: "root" })
export class CheckpointFailureService {
  private checkpointFailureUrl = "rest/Trend/scangrouptop10checkpointfailures";

  constructor(private http: HttpClient) {}

  public checkpointFailureResultsList(
    scanGroupId: string
  ): Observable<CheckpointFailures[] | ObservableError> {
    return this.http
      .post<CheckpointFailures[]>(this.checkpointFailureUrl, {
        scanGroupId,
      })
      .pipe(catchObservableError("api:trend:apiError"));
  }
}
