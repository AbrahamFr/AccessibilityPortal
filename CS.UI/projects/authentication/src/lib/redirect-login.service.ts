import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { BaseUrlResolver, OrganizationIdService } from "navigation";

@Injectable({
  providedIn: "root",
})
export class RedirectLoginService {
  constructor(
    private router: Router,
    private baseUrlResolver: BaseUrlResolver,
    private organizationIdService: OrganizationIdService
  ) {}

  redirectToLogin() {
    const baseUrl = this.baseUrlResolver.baseUrl;
    const orgVirtualDir = this.organizationIdService.orgVirtualDir;

    this.router.navigate([
      "externalRedirect",
      {
        externalUrl:
          `${baseUrl}../${orgVirtualDir}/Login.aspx?ReturnUrl=` +
          encodeURIComponent(location.href),
      },
    ]);
  }
}
