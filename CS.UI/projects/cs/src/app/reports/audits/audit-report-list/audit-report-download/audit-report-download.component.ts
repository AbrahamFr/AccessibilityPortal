import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  Output,
  OnInit,
} from "@angular/core";
import { AuditService } from "../../audit.service";
import { AuditReport } from "../../../../data-types/types";
import { ObservableError } from "api-handler";
import { ESCAPE } from "@angular/cdk/keycodes";

@Component({
  selector: "app-audit-report-download",
  templateUrl: "./audit-report-download.component.html",
  styleUrls: ["./audit-report-download.component.scss"],
})
export class AuditReportDownloadComponent implements OnInit {
  @Input()
  selectedReport: AuditReport;
  @Input()
  error: ObservableError;
  @Output()
  closeDownloadError = new EventEmitter<null>();

  @HostListener("keydown", ["$event"])
  onkeydown(event: KeyboardEvent) {
    if (event.keyCode === ESCAPE) {
      this.onClose();
    }
  }

  constructor(private auditService: AuditService) {}

  ngOnInit() {}

  onClose = () => {
    this.closeDownloadError.emit();
    this.auditService.refreshList$.next(true);
  };
}
