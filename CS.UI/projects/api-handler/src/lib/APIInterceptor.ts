import { Injectable } from "@angular/core";
import {
  HttpEvent,
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
  HttpErrorResponse,
} from "@angular/common/http";
import { AuthenticationService, JwtStoreService, LoginService } from "authentication";
import { Observable, throwError, EMPTY } from "rxjs";
import "rxjs/add/operator/do";
import { map, catchError, tap, switchAll, take } from "rxjs/operators";

@Injectable({
  providedIn: "root",
})
export class APIInterceptor implements HttpInterceptor {
  constructor(
    private jwtStore: JwtStoreService,
    private loginService: LoginService,
    private authService: AuthenticationService
  ) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    if (
      req.url.startsWith("rest/Authentication/authenticate") ||
      req.url.startsWith("rest/User/create") ||
      req.url.startsWith("rest/User/sendPasswordResetLink") ||
      req.url.startsWith("rest/User/resetPassword") ||
      !req.url.startsWith("rest/")
    )
      return next.handle(req);

    const executeRequest = this.authService.jwtClaims$.pipe(
      take(1), // Get the latest token from the auth service.
      // Map the latest token to a request with the right header set.
      map((jwt) =>
        req.clone({
          url: req.url,
          withCredentials: true,
          headers: this.addHeaderForAuthentication(jwt, req),
        })
      ),
      map((apiReq) => next.handle(apiReq)),
      switchAll() // latest call wins
    );

    return executeRequest.pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status !== 401) return throwError(error);
        if (error.status === 401) {
          this.authService.setToken("");
          this.jwtStore.receiveJwtToken(null);
        }
        return this.loginService.redirectToLogin() ? EMPTY : executeRequest;
      })
    );
  }

  private addHeaderForAuthentication(jwt: string, req: HttpRequest<any>) {
    return req.headers.set("Authorization", `Bearer ${jwt}`);
  }
}
