import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  OnInit,
  Output,
} from "@angular/core";
import { HttpResponse } from "@angular/common/http";
import { AuditService } from "../../audit.service";
import { AuditReport } from "../../../../data-types/types";
import { LoadingService, ErrorHandlerService } from "api-handler";
import {
  NextActiveElementService,
  SetNextElementRefService,
} from "interactions";
import { ESCAPE } from "@angular/cdk/keycodes";

@Component({
  selector: "app-audit-report-delete",
  templateUrl: "./audit-report-delete.component.html",
  styleUrls: ["./audit-report-delete.component.scss"],
})
export class AuditReportDeleteComponent implements OnInit {
  @Input()
  selectedReport: AuditReport;
  @Output()
  closeDelete = new EventEmitter<null>();

  @HostListener("keydown", ["$event"])
  onkeydown(event: KeyboardEvent) {
    if (event.keyCode === ESCAPE) {
      this.onClose();
    }
  }

  constructor(
    private auditService: AuditService,
    private loadingService: LoadingService,
    private errorService: ErrorHandlerService,
    private nextActiveElementService: NextActiveElementService,
    private setNextElementRefService: SetNextElementRefService
  ) {}

  isLoading = this.loadingService.loading$;
  activeError$ = this.errorService.activeError$;

  ngOnInit() {}

  onClose = () => {
    this.closeDelete.emit();
    this.errorService.activeError$.next(null);
  };

  onDelete = () => {
    this.auditService
      .deleteAuditReport(this.selectedReport.auditReportId)
      .subscribe((res) => {
        if (res instanceof HttpResponse) {
          if (res.status == 200) {
            this.onClose();
            this.auditService.refreshList$.next(true);
            this.nextActiveElementService.nextActiveElement$.next(
              this.setNextElementRefService.setNextElementRef$.value
            );
          }
        }
        if (res.type == "ComplianceSheriffError") {
          const error = res;
          this.errorService.handleError(error);
        }
      });
  };
}
