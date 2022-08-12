import { Injectable } from "@angular/core";
import { Location } from "@angular/common";
import { Router, NavigationStart } from "@angular/router";
import { ChildRouteService } from "./child-route.service";
import { searchParamsToUrlParams } from "./urlParameterSerialization";
import { BehaviorSubject } from "rxjs";
import { Base64 } from "js-base64";
import { OrganizationIdService } from "./organization-id.service";
import { UrlDataParams, AppParams } from "./types";
import { IssueTrackerAppParams } from "cs-core";

@Injectable({
  providedIn: "root",
})
export class UrlParamsService {
  readonly activeUrlDataParams$ = new BehaviorSubject<UrlDataParams>({});

  readonly previousUrl$ = new BehaviorSubject<string>("");

  private readonly isNavPopstate$ = new BehaviorSubject<boolean>(false);

  constructor(
    private router: Router,
    private childRouteService: ChildRouteService,
    private location: Location,
    private organizationIdService: OrganizationIdService
  ) {}

  childRoute = this.childRouteService.getChildRoute();

  currentUrl() {
    return this.router.url.includes("?")
      ? this.router.url.split("?")[0]
      : this.router.url;
  }

  checkQueryParams() {
    return this.router.url.split("?").slice(1);
  }

  decodeHash(hash: string): IssueTrackerAppParams | undefined {
    let decodedHash: AppParams | undefined;
    try {
      decodedHash = JSON.parse(Base64.decode(decodeURIComponent(hash)));
    } catch (error) {
    } finally {
      return decodedHash as IssueTrackerAppParams | undefined;
    }
  }

  encodeAppData(appParams: IssueTrackerAppParams): string {
    return Base64.encode(JSON.stringify(appParams));
  }

  updateUrlParams(params: UrlDataParams) {
    this.childRoute = this.childRouteService.getChildRoute();
    const navigationUrl = !this.organizationIdService.useOrgVirtualDir
      ? this.currentUrl()
      : `${this.organizationIdService.orgVirtualDir}/${this.childRoute}`;

    this.router.navigate([navigationUrl], {
      queryParams: searchParamsToUrlParams(params),
    });
  }

  getIsNavPopstate(): boolean {
    return this.isNavPopstate$.value;
  }

  closeIsNavePopstate(): void {
    this.isNavPopstate$.next(false);
  }
  openIsNavPopstate(event: any): void {
    if (
      event instanceof NavigationStart &&
      event.navigationTrigger == "popstate"
    ) {
      this.isNavPopstate$.next(true);
    }
  }

  getPreviousUrlPath() {
    return this.previousUrl$.value.split("?")[0];
  }
  getPreviousURL() {
    return this.previousUrl$.value;
  }
}
