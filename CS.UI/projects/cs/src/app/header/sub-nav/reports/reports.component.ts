import { Component, OnInit, AfterContentChecked, Input } from "@angular/core";
import { AuthenticationService } from "authentication";

@Component({
  selector: "app-reports",
  templateUrl: "./reports.component.html",
  styleUrls: ["./reports.component.scss"],
})
export class ReportsComponent implements OnInit, AfterContentChecked {
  @Input()
  homePath: string;
  @Input()
  orgVirtualDir: string | null;
  @Input()
  childRoute: string;

  hasTrendPerm: boolean;
  hasIssueTrackerPerm: boolean;
  hasAuditPerm: boolean;
  reportsUrl: string | null;

  constructor(private authService: AuthenticationService) {}

  ngOnInit() {}

  ngAfterContentChecked() {
    this.initializeData();
  }

  initializeData() {
    this.hasTrendPerm = this.authService.permissionCheck("Trend");
    this.hasIssueTrackerPerm = this.authService.permissionCheck("IssueTracker");
    this.hasAuditPerm = this.authService.permissionCheck("Audit");
    this.reportsUrl = `${this.orgVirtualDir}/Reports`;
  }
}
