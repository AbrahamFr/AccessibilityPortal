import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import {
  APIResponse,
  catchObservableError,
  makeObservableError,
} from "api-handler";
import { of, throwError } from "rxjs";
import {
  catchError,
  map,
  publishReplay,
  refCount,
  startWith,
} from "rxjs/operators";
import { EmailResetRequest } from "../email-submission/emailResetRequest";

@Injectable({
  providedIn: "root",
})
export class PasswordResetService {
  private sendPasswordEmailUrl: string = `rest/User/sendPasswordResetLink`;

  constructor(private http: HttpClient) {}

  sendPasswordResetEmail(emailResetRequest: EmailResetRequest) {
    return this.http
      .post<any>(this.sendPasswordEmailUrl, emailResetRequest)
      .pipe(
        startWith(0),
        map((response) => response as APIResponse),
        catchError((response) => {
          if (response instanceof HttpErrorResponse) {
            return of(
              makeObservableError(response, "api:user:passwordReset:apiError")
            );
          } else {
            return throwError(response);
          }
        }),
        catchObservableError("api:user:passwordReset:apiError"),
        publishReplay(1),
        refCount()
      );
  }
}
