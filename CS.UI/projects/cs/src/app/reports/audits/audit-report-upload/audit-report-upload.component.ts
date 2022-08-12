import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  HostListener,
} from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { tap } from "rxjs/operators";
import { HttpResponse } from "@angular/common/http";
import { AuditService } from "../audit.service";
import { LoadingService, ErrorHandlerService } from "api-handler";
import { AuthenticationService } from "authentication";
import {
  validateFileSize,
  requiredFileType,
} from "../../../file-upload/file-validation";
import { ServerValidationError } from "./server-validation-error-translation";
import { ESCAPE } from "@angular/cdk/keycodes";

@Component({
  selector: "app-audit-report-upload",
  templateUrl: "./audit-report-upload.component.html",
  styleUrls: ["./audit-report-upload.component.scss"],
})
export class AuditReportUploadComponent implements OnInit {
  readonly maxFileSize: number = 100;

  @Output()
  closeUpload = new EventEmitter<null>();

  @HostListener("keydown", ["$event"])
  onkeydown(event: KeyboardEvent) {
    if (event.keyCode === ESCAPE) {
      this.onClose(this.fileUploadForm);
    }
  }

  constructor(
    private auditService: AuditService,
    private authService: AuthenticationService,
    private fb: FormBuilder,
    private loadingService: LoadingService,
    private errorService: ErrorHandlerService
  ) {}

  isLoading = this.loadingService.loading$;
  activeError$ = this.errorService.activeError$;

  serverValidationError$ = this.errorService.serverValidationError$.pipe(
    tap((err) => {
      if (err) {
        this.fileUploadForm.controls[err.field].setErrors({
          [err.error]: true,
        });
      }
    })
  );

  fileUpload: File | null = null;
  fileReportName: string | "";
  fileUploadForm = this.fb.group({
    reportFile: [
      "",
      Validators.compose([
        Validators.required,
        validateFileSize(this.maxFileSize),
        requiredFileType("doc,docx,xls,xlsx,pdf,zip,txt"),
      ]),
    ],
    reportName: [
      "",
      Validators.compose([
        Validators.required,
        Validators.minLength(5),
        Validators.maxLength(100),
      ]),
    ],
    reportType: ["", Validators.required],
    reportDescription: [""],
  });

  // convenience getter for easy access to form fields
  get form() {
    return this.fileUploadForm.controls;
  }

  ngOnInit() {}

  onFileInput(files: FileList) {
    const file = files && files.item(0);
    this.fileUpload = file;
    if (!this.fileReportName) {
      this.fileReportName = files.item(0) ? files.item(0)!.name : "";
      this.fileReportName =
        this.fileReportName.substr(0, this.fileReportName.lastIndexOf(".")) ||
        this.fileReportName;
    }
  }

  onClose = (form: FormGroup) => {
    this.closeUpload.emit(null);
    this.errorService.activeError$.next(null);
    this.errorService.serverValidationError$.next(null);
    if (form) {
      form.reset();
    }
  };

  onSubmit = () => {
    const value = this.fileUploadForm.value;
    if (value) {
      this.auditService
        .uploadAuditReport(this.fileUpload, value)
        .subscribe((res) => {
          if (res instanceof HttpResponse) {
            if (res.status == 201) {
              this.onClose(this.fileUploadForm);
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
