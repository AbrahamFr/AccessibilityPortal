import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { catchError, map, tap } from "rxjs/operators";
import { EMPTY, of } from "rxjs";
import { AuthRequest } from "./types";
import { JwtStoreService } from "./jwt-store.service";
import { LoginMessagingService } from "./login-messaging.service";

@Injectable({
  providedIn: "root",
})
export class ApiAuthenticationService {
  constructor(
    private http: HttpClient,
    private jwtStore: JwtStoreService,
    private loginMessagingService: LoginMessagingService
  ) {}

  authUrl: string = `rest/Authentication/authenticate`;

  authenticateUser() {
    if (!this._authRequestUser.UserName) {
      console.log("No username in Auth Request");
      return EMPTY;
    }
    const authRequestUser: AuthRequest = this._authRequestUser;
    this._authRequestUser = {
      UserName: "",
      Password: "",
      OrganizationId: "",
      AuthenticationType: "",
    };

    return this.http
      .post<any>(this.authUrl, authRequestUser, {
        observe: "response",
        withCredentials: true,
      })
      .pipe(
        map((response) => response.body),
        catchError((error) => {
          if (error?.error?.errorCode) {
            this.loginMessagingService.addLoginErrors(error.error.errorCode);
          }
          return of(error);
        }),
        tap({
          next: (responseBody: any) => {
            var jwt = responseBody.data.authToken;
            this.jwtStore.receiveJwtToken(jwt);
          },
        })
      );
  }

  private _authRequestUser: AuthRequest;

  setAuthRequestUser(authRequest: AuthRequest) {
    this._authRequestUser = authRequest;
  }
}
