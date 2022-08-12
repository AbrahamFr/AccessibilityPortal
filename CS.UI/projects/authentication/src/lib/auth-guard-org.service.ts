import { Injectable } from "@angular/core";
import {
  CanActivate,
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  CanActivateChild,
  UrlSegment,
  UrlTree,
  UrlSegmentGroup,
  PRIMARY_OUTLET,
} from "@angular/router";
import { AuthenticationService } from "./auth.service";
import { Observable, of } from "rxjs";
import { flatMap, map, tap } from "rxjs/operators";
import { OrganizationIdService, OrganizationResolver } from "navigation";

@Injectable()
export class AuthGuardOrg implements CanActivate, CanActivateChild {
  constructor(
    private authService: AuthenticationService,
    private router: Router,
    private orgResolver: OrganizationResolver,
    private organizationIdService: OrganizationIdService
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> {
    // take the organization virtual Directory from the URL and place in local storage
    this.orgResolver.getOrgIdFromRouteSnapshot(route);

    return this.authService.jwtClaims$.pipe(
      flatMap((claim) => {
        return Boolean(claim) ? of(true) : of(false);
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
    const urlChildPath = urlSegment[2].path;

    const isPermissioned = this.authService.permissionCheck(urlChildPath);
    if (isPermissioned) {
      return true;
    }

    this.router.navigate([
      `${this.organizationIdService.orgVirtualDir}/Error`,
      { path: urlChildPath },
    ]);
  }
}
