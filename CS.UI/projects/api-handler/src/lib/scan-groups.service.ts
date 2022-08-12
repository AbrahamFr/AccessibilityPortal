import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CreateScanGroupParams, UpdateScanGroupNameParams } from "cs-core";
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
export class ScanGroupsService {
  private createScanGroupUrl = "rest/ScanGroup/create";
  private updateScanGroupNameUrl = "rest/ScanGroup/updateName";

  constructor(private http: HttpClient) {}

  getScanGroupId(): number {
    const scanGroupId =
      localStorage.getItem("scanGroupId") == null
        ? "0"
        : localStorage.getItem("scanGroupId")!;
    return parseInt(scanGroupId);
  }

  createScanGroup(
    createScanGroupParams: CreateScanGroupParams
  ): Observable<APIResponse | ObservableError> {
    return this.http.post(this.createScanGroupUrl, createScanGroupParams).pipe(
      startWith(0),
      map((response) => response as APIResponse),
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(
            makeObservableError(response, "api:scanGroup:create:error")
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

  updateScanGroupName(
    updateScanGroupNameParams: UpdateScanGroupNameParams
  ): Observable<APIResponse | ObservableError> {
    return this.http.put(this.updateScanGroupNameUrl, updateScanGroupNameParams).pipe(
      startWith(0),
      map((response) => response as APIResponse),
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(
            makeObservableError(response, "api:scanGroup:create:error")
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
