import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AddScanGroupScansParams } from "cs-core";
import { Observable, of, throwError } from "rxjs";
import {
  catchError,
  map,
  publishReplay,
  refCount,
  startWith,
} from "rxjs/operators";
import {
  catchObservableError,
  makeObservableError,
  ObservableError,
} from "./observable-error";
import { APIResponse } from "./types";

@Injectable({
  providedIn: "root",
})
export class ScanGroupsScansService {
  private addScanGroupScansUrl = "rest/ScanGroupScans/add";

  constructor(private http: HttpClient) {}

  addScanGroupScans(
    addScanGroupScans: AddScanGroupScansParams
  ): Observable<APIResponse | ObservableError> {
    return this.http.post(this.addScanGroupScansUrl, addScanGroupScans).pipe(
      startWith(0),
      map((response) => response as APIResponse),
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(
            makeObservableError(response, "api:scanGroupScans:add:error")
          );
        } else {
          return throwError(response);
        }
      }),
      catchObservableError("api:scanGroup:create:apiError"),
      publishReplay(1),
      refCount()
    );
  }
}
