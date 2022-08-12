import { NgModule } from "@angular/core";

import { AuditComponent } from "./audit.component";
import { AuditService } from "./audit.service";
import { AuditReportListComponent } from "./audit-report-list/audit-report-list.component";
import { AuditReportDeleteComponent } from "./audit-report-list/audit-report-delete/audit-report-delete.component";
import { AuditReportDownloadComponent } from "./audit-report-list/audit-report-download/audit-report-download.component";
import { AuditReportEditComponent } from "./audit-report-list/audit-report-edit/audit-report-edit.component";
import { AuditReportUploadComponent } from "./audit-report-upload/audit-report-upload.component";
import { FilterAuditReportOptionsPipe } from "./filter-report-types";
import { SharedModule } from "../../shared/shared.module";
import { AuditsRoutingModule } from "./audits-routing.module";
import { HttpClient } from "@angular/common/http";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { TranslateModule, TranslateLoader } from "@ngx-translate/core";
import { AppOnlineComponent } from "../../app-online/app-online.component";

export function createAuditTranslateLoader(http: HttpClient) {
  return new TranslateHttpLoader(
    http,
    "./app/reports/audits/i18n/",
    "-lang.json"
  );
}

@NgModule({
  declarations: [
    AuditComponent,
    AuditReportListComponent,
    AuditReportDeleteComponent,
    AuditReportDownloadComponent,
    AuditReportEditComponent,
    AuditReportUploadComponent,
    FilterAuditReportOptionsPipe,
    AppOnlineComponent,
  ],
  imports: [
    SharedModule,
    AuditsRoutingModule,
    TranslateModule.forChild({
      loader: {
        provide: TranslateLoader,
        useFactory: createAuditTranslateLoader,
        deps: [HttpClient],
      },
      isolate: true,
    }),
  ],
  providers: [AuditService],
})
export class AuditsModule {}
