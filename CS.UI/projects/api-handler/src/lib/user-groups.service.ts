import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UpdateUserGroupNameParams } from 'cs-core';
import { Observable, of, throwError } from 'rxjs';
import { catchError, map, publishReplay, refCount, startWith } from 'rxjs/operators';
import { catchObservableError, makeObservableError, ObservableError } from './observable-error';
import { APIResponse } from './types';

@Injectable({
  providedIn: 'root'
})
export class UserGroupsService {
  private updateUserGroupNameUrl = "rest/UserGroup/updateName";

  public get userGroupId():number {
    return Number(localStorage.getItem("userGroupId"));
  }

  constructor(private http: HttpClient) { }

  updateUserGroupName(
    updateUserGroupNameParams: UpdateUserGroupNameParams
  ): Observable<APIResponse | ObservableError> {
    return this.http.put(this.updateUserGroupNameUrl, updateUserGroupNameParams).pipe(
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
