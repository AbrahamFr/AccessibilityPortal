import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  OnInit,
  Output,
} from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { tap } from "rxjs/operators";
import { HttpResponse } from "@angular/common/http";
import { AuditService } from "../../audit.service";
import { AuditReport } from "../../../../data-types/types";
import { LoadingService, ErrorHandlerService } from "api-handler";
import { ServerValidationError } from "./server-validation-error-translation";
import { ESCAPE } from "@angular/cdk/keycodes";

@Component({
  selector: "app-audit-report-edit",
  templateUrl: "./audit-report-edit.component.html",
  styleUrls: ["./audit-report-edit.component.scss"],
})
export class AuditReportEditComponent implements OnInit {
  @Input()
  selectedReport: AuditReport;
  @Output()
  closeEdit = new EventEmitter<null>();

  @HostListener("keydown", ["$event"])
  onkeydown(event: KeyboardEvent) {
    if (event.keyCode === ESCAPE) {
      this.onClose(this.editReportForm);
    }
  }

  constructor(
    private auditService: AuditService,
    private fb: FormBuilder,
    private loadingService: LoadingService,
    private errorService: ErrorHandlerService
  ) {}

  isLoading = this.loadingService.loading$;
  activeError$ = this.errorService.activeError$;

  serverValidationError$ = this.errorService.serverValidationError$.pipe(
    tap((err) => {
      if (err) {
        this.editReportForm.controls[err.field].setErrors({
          [err.error]: true,
        });
      }
    })
  );

  editReportForm = this.fb.group({
    reportName: [
      "",
      Validators.compose([
        Validators.required,
        Validators.minLength(5),
        Validators.maxLength(100),
      ]),
    ],
    auditTypeId: [""],
    reportDescription: [""],
  });

  ngOnInit() {
    if (this.selectedReport) {
      this.editReportForm.patchValue({
        reportName: this.selectedReport.reportName,
        auditTypeId: this.selectedReport.auditTypeId,
        reportDescription: this.selectedReport.reportDescription,
      });
    }
  }

  onClose = (form: FormGroup) => {
    this.closeEdit.emit(null);
    this.errorService.activeError$.next(null);
    if (form) {
      form.reset();
    }
  };

  onSubmit = () => {
    const value = this.editReportForm.value;
    if (value) {
      this.auditService
        .editAuditReportData({
          ...value,
          ...{ auditReportId: this.selectedReport.auditReportId },
        })
        .subscribe((res) => {
          if (res instanceof HttpResponse) {
            if (res.status == 200) {
              this.onClose(this.editReportForm);
              this.auditService.refreshList$.next(true);
            }
          }
          if (res.type == "ComplianceSheriffError") {
            const error = res;
            this.errorService.handleError(error, ServerValidationError);
          }
        });
    }
  };
}
