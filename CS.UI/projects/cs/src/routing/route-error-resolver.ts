import { Injectable } from "@angular/core";
import { Resolve, Router, RouterStateSnapshot, ActivatedRouteSnapshot, UrlTree, UrlSegmentGroup, UrlSegment, PRIMARY_OUTLET } from "@angular/router";
import { of, Observable } from "rxjs"

@Injectable()
export class RouteErrorResolver implements Resolve<Observable<string>> {
    constructor(
        private router: Router,
    ) {}

    resolve(_: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
      const urlTree: UrlTree = this.router.parseUrl(state.url)
      const urlGroup: UrlSegmentGroup = urlTree.root.children[PRIMARY_OUTLET]
      const urlSegment: UrlSegment[] = urlGroup.segments
      const urlChildPath = urlSegment[1].parameters.path;

      if (urlChildPath) {
        return of(`You currently do not have access to the ${urlChildPath} Report.  Please contact your Administrator to request access.`)
      }
        return of("The route path you're attempting to access does not exist.")
    }
}