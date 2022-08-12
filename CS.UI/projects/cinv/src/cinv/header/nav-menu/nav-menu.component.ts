import { Component, OnInit } from "@angular/core";
import {
  Router,
  UrlTree,
  UrlSegmentGroup,
  PRIMARY_OUTLET,
  UrlSegment,
  NavigationEnd,
} from "@angular/router";
import { LoginService } from "authentication";
import { map } from "rxjs/operators";

@Component({
  selector: "cinv-nav-menu",
  templateUrl: "./nav-menu.component.html",
  styleUrls: ["./nav-menu.component.scss"],
})
export class NavMenuComponent {
  loginState: string = "navigate";
  isLoggedIn: boolean;

  isActiveTab(urlSegment, exact) {
    return this.router.isActive(urlSegment, exact)
  }

  constructor(private router: Router, private loginService: LoginService) {
    this.setLoginStateFromUrl(router);
  }

  private setLoginStateFromUrl(router: Router) {
    router.events.subscribe((val) => {
      if (val instanceof NavigationEnd) {
        const urlTree: UrlTree = this.router.parseUrl(val.urlAfterRedirects);
        const urlGroup: UrlSegmentGroup = urlTree.root.children[PRIMARY_OUTLET];
        if (!urlGroup || urlGroup.segments.length < 1) {
          return;
        }

        const urlSegment: UrlSegment[] = urlGroup.segments;
        const urlChildPath = urlSegment[0].path;
        switch (urlChildPath) {
          case "landing":
            this.loginState = "navigate";
            break;
          case "login":
            this.loginState = "login";
            break;
          default:
            this.loginService
              .verifyLoggedIn()
              .pipe(
                map((loggedIn) => {
                  this.loginState = loggedIn ? "authenticated" : "navigate";
                })
              )
              .subscribe();
            break;
        }
      }
    });
  }
}
