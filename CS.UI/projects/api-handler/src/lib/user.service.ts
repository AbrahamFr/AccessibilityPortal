import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpResponse,
} from "@angular/common/http";
import { CreateUserParams } from "cs-core";
import {
  ObservableError,
  makeObservableError,
  catchObservableError,
} from "./observable-error";
import {
  startWith,
  map,
  catchError,
  publishReplay,
  refCount,
} from "rxjs/operators";
import { throwError, of, Observable } from "rxjs";
import { APIResponse } from "./types";
import { ResetPasswordRequest, UpdatePasswordRequest, UpdateUserRequest, User } from "authentication";

@Injectable({
  providedIn: "root",
})
export class UserService {
  private createUserUrl = `rest/User/create`;
  private resetPasswordUrl = `rest/User/resetPassword`;
  private getUserByUserNameUrl = `rest/User/getUserByUserName/`;
  private updateUserUrl = `rest/User/update`;
  private updatePasswordUrl = `rest/User/updatePassword`;

  constructor(
    private http: HttpClient
  ) {}

  updateUser(
    updateUserRequest: UpdateUserRequest
  ): Observable<APIResponse | ObservableError> {
    return this.http.put(this.updateUserUrl, updateUserRequest).pipe(
      startWith(0),
      map((response) => response as any),
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(makeObservableError(response, "api:user:Create:apiError"));
        } else {
          return throwError(response);
        }
      })
    );
  }

  createUser(
    createUserParams: CreateUserParams
  ): Observable<APIResponse | ObservableError> {
    return this.http.post(this.createUserUrl, createUserParams).pipe(
      startWith(0),
      map((response) => response as APIResponse),
      catchError((response) => {
        if (response instanceof HttpErrorResponse && response.status === 409) {
          return of(
            makeObservableError(response, "api:User:create:userAlreadyExists")
          );
        } else {
          return throwError(response);
        }
      }),
      catchObservableError("api:Create:user:apiError"),
      publishReplay(1),
      refCount()
    );
  }

  getUserByUserName(
    userName: string
  ): Observable<APIResponse | ObservableError> {
    return this.http.get(this.getUserByUserNameUrl + userName).pipe(
      startWith(0),
      map((response) => response as any),
      catchError((response) => {
        if (
          (response instanceof HttpErrorResponse && response.status === 409) ||
          response.status === 400
        ) {
          return of(makeObservableError(response, "api:user:Create:apiError"));
        } else {
          return throwError(response);
        }
      })
    );
  }

  resetUserPassword(
    resetPasswordRequest: ResetPasswordRequest
  ): Observable<APIResponse | ObservableError> {
    return this.http.post(this.resetPasswordUrl, resetPasswordRequest).pipe(
      startWith(0),
      map((response) => response as any),
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(makeObservableError(response, "api:user:Create:apiError"));
        } else {
          return throwError(response);
        }
      })
    );
  }

  updatePassword(
    updatePasswordRequest: UpdatePasswordRequest
  ): Observable<APIResponse | ObservableError> {
    return this.http.post(this.updatePasswordUrl, updatePasswordRequest).pipe(
      startWith(0),
      map((response) => response as any),
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(makeObservableError(response, "api:user:Create:apiError"));
        } else {
          return throwError(response);
        }
      })
    );
  }
}
