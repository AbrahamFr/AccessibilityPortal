import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { Observable, BehaviorSubject, Subscription, Subject, of } from "rxjs";
import { flatMap, switchAll } from "rxjs/operators";
import { ApiAuthenticationService } from "./api-authentication.service";
import { AuthenticationService, JwtClaims } from "./auth.service";
import { JwtStoreService } from "./jwt-store.service";
import { AuthRequest } from "./types";


@Injectable({
  providedIn: "root",
})
export class LoginService {
  jwtClaims: Subscription;

  constructor(
    private authService: AuthenticationService,
    private router: Router,
    private apiAuthService: ApiAuthenticationService,
    private jwtStore: JwtStoreService
  ) {}

  getJwtClaims() {
    return this.authService.jwtClaims$;
  }

  getUserName() {
    return localStorage.getItem("userName");
  }
  loginWith(authRequest: AuthRequest): Observable<string> {
    this.apiAuthService.setAuthRequestUser(authRequest);
    localStorage.setItem("useLoginService", "true");
    this.jwtStore.receiveJwtToken("");
    return this.authService.jwtClaims$;
  }

  logout() {
    localStorage.clear();
    this.jwtStore.receiveJwtToken(null);
  }

  verifyLoggedIn(): Observable<boolean> {
    const token = localStorage.getItem("token");
    if (!token) {
      return of(false);
    }
    const userName = this.getUserName();
    return this.getJwtClaims().pipe(
      flatMap((claim) => {
        if (!claim) {
          return of(false);
        }


        const claims = this.authService.parseJwtClaims(token);

        if (claims["userName"] !== userName) {
          return of(false);
        }

        return of(true);
      })
    );
  }

  redirectToLogin() {
    const useLoginService = localStorage.getItem("useLoginService");
    if (!useLoginService || useLoginService !== "true") {
      return false;
    }
    this.router.navigate(["/login"]);
    return true;
  }
}
