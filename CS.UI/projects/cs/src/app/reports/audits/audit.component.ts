import { Component, OnInit } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { BehaviorSubject } from "rxjs";
import { AuthenticationService, JwtStoreService } from "authentication";
import { AuditReport } from "../../data-types/types";
import { ObservableError } from "api-handler";
import { AuditService } from "./audit.service";
import {
  PreviousActiveElementService,
  NextActiveElementService,
} from "interactions";
import { AppSettings } from "../../constants/app-settings";
import { TranslateService } from "@ngx-translate/core";

@Component({
  selector: "app-audit",
  templateUrl: "./audit.component.html",
  styleUrls: ["./audit.component.scss"],
})
export class AuditComponent implements OnInit {
  constructor(
    private auditService: AuditService,
    private authService: AuthenticationService,
    private jwtStore: JwtStoreService,
    private activeElementService: PreviousActiveElementService,
    private nextActiveElementService: NextActiveElementService,
    private titleService: Title,
    private readonly translate: TranslateService
  ) {
    this.titleService.setTitle(AppSettings.PAGETITLE + " (Reports - Audit)");
    translate.addLangs(["en"]),
      translate.setDefaultLang("en"),
      translate.use("en");
  }

  canUploadReport: boolean = false;

  isUploadActive: boolean = false;
  readonly activeUpload$ = new BehaviorSubject<boolean>(this.isUploadActive);

  auditReports: AuditReport[] | ObservableError;

  refreshList$ = this.auditService.refreshList$.subscribe((refresh) =>
    refresh ? this.onRefreshReportsList() : null
  );

  nextActiveElement$ = this.nextActiveElementService.nextActiveElement$;

  ngOnInit() {
    this.canUploadReport = this.authService.hasAuditUploadPermission();
    this.onRefreshReportsList();
  }

  onUploadClick() {
    this.nextActiveElementService.nextActiveElement$.next(null);
    this.activeElementService.previousActiveElement$.next(
      document.activeElement as HTMLElement
    );
    this.isUploadActive = !this.isUploadActive;
    this.activeUpload$.next(this.isUploadActive);
  }

  onCloseUpload() {
    this.isUploadActive = !this.isUploadActive;
    this.activeUpload$.next(false);
    this.activeElementService.setFocus();
  }

  onRefreshReportsList() {
    this.jwtStore.receiveJwtToken(null);
    this.auditService
      .getAuditReportsList()
      .subscribe((reports) => (this.auditReports = reports));
  }
}
