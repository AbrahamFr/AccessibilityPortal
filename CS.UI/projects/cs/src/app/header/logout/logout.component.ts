import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { LoginService } from "authentication";
import { BaseUrlResolver, OrganizationIdService } from "navigation";

@Component({
  selector: "app-logout",
  templateUrl: "./logout.component.html",
  styleUrls: ["./logout.component.css"],
})
export class LogoutComponent implements OnInit {
  constructor(
    private loginService: LoginService,
    private router: Router,
    private baseUrlResolver: BaseUrlResolver,
    private organizationIdService: OrganizationIdService
  ) {}

  ngOnInit() {
    this.router.navigate([
      "externalRedirect",
      {
        externalUrl: `${this.baseUrlResolver.baseUrl}../${this.organizationIdService.orgVirtualDir}/Login.aspx`,
      },
    ]);
    this.loginService.logout();
  }
}
