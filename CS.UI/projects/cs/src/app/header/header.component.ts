import { Component, AfterContentChecked, OnInit } from "@angular/core";
import {
  BaseUrlResolver,
  ChildRouteService,
  OrganizationIdService,
} from "navigation";

@Component({
  selector: "app-header",
  templateUrl: "./header.component.html",
  styleUrls: ["./header.component.scss"],
})
export class HeaderComponent implements OnInit, AfterContentChecked {
  homePath: string;
  orgVirtualDir: string | null;
  baseUrl: string;
  childRoute: string;
  skipToMainContentUrl: string | null;

  constructor(
    private baseUrlResolver: BaseUrlResolver,
    private childRouteService: ChildRouteService,
    private organizationIdService: OrganizationIdService
  ) {}

  ngOnInit() {}

  ngAfterContentChecked() {
    this.initializeHeaderData();
  }

  initializeHeaderData() {
    this.orgVirtualDir = this.organizationIdService.orgVirtualDir;
    this.baseUrl = this.baseUrlResolver.baseUrl;
    this.homePath = `${this.baseUrl}../${this.orgVirtualDir}`;
    this.childRoute = this.childRouteService.getChildRoute();
    this.skipToMainContentUrl = `${this.orgVirtualDir}/${this.childRoute}#main-content`;
  }
}
