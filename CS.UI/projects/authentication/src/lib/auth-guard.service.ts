import { Injectable } from "@angular/core";
import {
  CanActivateChild,
  CanActivate,
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  UrlTree,
  UrlSegmentGroup,
  UrlSegment,
  PRIMARY_OUTLET,
} from "@angular/router";
import { Observable } from "rxjs";
import { tap } from "rxjs/operators";
import { AuthenticationService } from "./auth.service";
import { LoginService } from "./login.service";

@Injectable({
  providedIn: "root",
})
export class AuthGuard implements CanActivate, CanActivateChild {
  constructor(
    private authService: AuthenticationService,
    private router: Router,
    private loginService: LoginService
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> {
    return this.loginService.verifyLoggedIn().pipe(
      tap((loggedIn) => {
        if (!loggedIn) {
          this.router.navigate(["/login"]);
        }
      })
    );
  }

  canActivateChild(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean | any {
    const urlTree: UrlTree = this.router.parseUrl(state.url);
    const urlGroup: UrlSegmentGroup = urlTree.root.children[PRIMARY_OUTLET];
    const urlSegment: UrlSegment[] = urlGroup.segments;
    const urlChildPath = urlSegment[0].path;

    const isPermissioned = this.authService.permissionCheck(urlChildPath);
    if (isPermissioned) {
      return true;
    }
  }
}
