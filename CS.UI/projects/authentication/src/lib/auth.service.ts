import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import {
  catchError,
  shareReplay,
  map,
  tap,
  take,
  ignoreElements,
  filter,
  concatAll,
  distinctUntilChanged,
} from "rxjs/operators";
import "rxjs/add/operator/do";
import { EMPTY, of, concat } from "rxjs";
import { OrganizationIdService } from "navigation";
import { JwtStoreService } from "./jwt-store.service";
import { Claims, AuthMode } from "./types";
import { RedirectLoginService } from "./redirect-login.service";
import { ApiAuthenticationService } from "./api-authentication.service";

export type JwtClaims = Record<string, any>;
const xAuthenticationTokenHeader = "X-Authentication-Token";

@Injectable({
  providedIn: "root",
})
export class AuthenticationService {
  constructor(
    private jwtStore: JwtStoreService,
    private http: HttpClient,
    private organizationIdService: OrganizationIdService,
    private redirectLoginService: RedirectLoginService,
    private apiAuthService: ApiAuthenticationService
  ) {}

  private authMode: AuthMode = AuthMode.AuthWebForms;

  public tryReAuthenticateToken() {
    if (this.authMode === AuthMode.AuthWebAPI) {
      return this.apiAuthService.authenticateUser();
    }

    const url: string = `../${this.organizationIdService.orgVirtualDir}/Authenticate.ashx`;
    return this.http
      .get<any>(url, {
        observe: "response",
        withCredentials: true,
      })
      .pipe(
        catchError((error) => {
          this.redirectLoginService.redirectToLogin();
          return EMPTY;
        }),
        filter((response) => response.headers.has(xAuthenticationTokenHeader)),
        map((response) => response.headers.get(xAuthenticationTokenHeader)!),
        tap({
          next: (jwt: string) => {
            this.jwtStore.receiveJwtToken(jwt);
          },
        })
      );
  }

  public readonly jwtToken$ = this.jwtStore.authenticationToken$.pipe(
    //distinctUntilChanged(), // should not re-emit, if Token does not change
    //Commented out distinctUntilChanged because it was retaining the expired jwt token in Compliance Sheriff
    map((value) => {
      if (value) return of(value); // We have a token! Why are we talking anymore?

      const token = localStorage.getItem("token");
      if (token && token !== "") return of(token); // on page refresh get token from local storage
      return concat(
        of(""), // prevent the old bad token from being replayed
        this.tryReAuthenticateToken().pipe(ignoreElements()) // JWT gets re-added via this.jwtStore.authenticationToken$
      );
    }),
    concatAll(),
    shareReplay(1), // all listenters will get the same token
    filter((jwt) => jwt !== "") // prevents subscribers from seeing the "in progress" jwt value
  );

  public get jwtClaims$() {
    return this.jwtToken$.pipe(
      take(1),
      map((jwt) => {
        this.setToken(jwt);
        return jwt;
      }),
      tap((token) => {
        if (token) {
          const claims = this.parseJwtClaims(token);
          this.setSession(claims as Claims);
        }
      })
    );
  }

  public setToken(jwt: string) {
    if (!jwt) {
      localStorage.removeItem("token");
    }
    localStorage.setItem("token", jwt);
  }
  private setSession(jwtClaim: Claims) {
    localStorage.setItem("userName", jwtClaim.userName);
    if (jwtClaim.scanGroupId) {
      localStorage.setItem("scanGroupId", jwtClaim.scanGroupId.toString());
    }
    if (jwtClaim.userGroupId) {
      localStorage.setItem("userGroupId", jwtClaim.userGroupId.toString());
    }
    localStorage.setItem(
      "roles",
      jwtClaim.roles.map((role: string) => role).join(",")
    );
  }

  public parseJwtClaims(token: string): JwtClaims {
    return JSON.parse(atob(token.split(".")[1])) as JwtClaims;
  }

  public isAdministrator(): boolean {
    const roles = localStorage.getItem("roles");
    return roles ? roles.includes("Administrator") : false;
  }

  public permissionCheck(role: string): boolean {
    const userRoles = localStorage.getItem("roles");
    if (userRoles) {
      switch (role) {
        case "Audits":
          return userRoles.includes("Audit");
        default:
          return userRoles.includes(role);
      }
    }
    return false;
  }

  public hasExportIssuesPermission(): boolean {
    const userRoles = localStorage.getItem("roles");
    if (userRoles && userRoles.includes("IssueTrackerReport")) {
      return true;
    }
    return false;
  }

  public hasAuditUploadPermission(): boolean {
    const userRoles = localStorage.getItem("roles");
    if (userRoles && userRoles.includes("AuditReportCreator")) {
      return true;
    }
    return false;
  }

  public setAuthMode(authMode: AuthMode) {
    this.authMode = authMode;
  }
}
