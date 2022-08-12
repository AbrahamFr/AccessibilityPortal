import { Component, HostListener, Input } from "@angular/core";
import { HttpResponse } from "@angular/common/http";
import { FormBuilder, Validators, FormGroup } from "@angular/forms";
import { DatePipe } from "@angular/common";
import { ESCAPE } from "@angular/cdk/keycodes";
import { tap } from "rxjs/operators";

import { ExportReportParams } from "cs-core";
import { ErrorHandlerService, LoadingService } from "api-handler";
import { IssueTrackerService } from "../issue-tracker.service";
import { IssueTrackerParamService } from "../issue-tracker-param.service";
import { IssueTrackerIssues } from "../types";
import { InteractionsService } from "interactions";

@Component({
  selector: "app-issue-tracker-export",
  templateUrl: "./issue-tracker-export.component.html",
  styleUrls: ["./issue-tracker-export.component.scss"],
})
export class IssueTrackerExportComponent {
  @Input()
  selectedReport: IssueTrackerIssues;
  @Input()
  issues: IssueTrackerIssues;

  @HostListener("keydown", ["$event"])
  onkeydown(event: KeyboardEvent) {
    if (event.keyCode === ESCAPE) {
      this.onClose(this.issueTrackerExportsFileDownloadForm);
    }
  }

  constructor(
    private fb: FormBuilder,
    private errorService: ErrorHandlerService,
    private issueTrackerParamService: IssueTrackerParamService,
    private loadingService: LoadingService,
    private issueTrackerService: IssueTrackerService,
    private interactionsService: InteractionsService
  ) {}

  isLoading = this.loadingService.loading$;
  activeError$ = this.errorService.activeError$;
  useCInvStyles = this.interactionsService.useCInvStyles;

  issueTrackerExportsFileDownloadForm = this.fb.group({
    issuesType: [""],
    reportFormat: ["", Validators.required],
    reportName: ["", Validators.required],
  });

  reportFormat: string = "";
  reportName: string =
    "Issue_Tracker_" +
    new DatePipe("en-US").transform(new Date(), "dd_MMM_yyyy");

  serverValidationError$ = this.errorService.serverValidationError$.pipe(
    tap((err) => {
      if (err) {
        this.issueTrackerExportsFileDownloadForm.controls[err.field].setErrors({
          [err.error]: true,
        });
      }
    })
  );

  setExportButtonFocus() {
    const exportBtn = document.getElementById("btnexport") as HTMLInputElement;
    if (exportBtn) {
      setTimeout(() => {
        exportBtn.focus();
      }, 0);
    }
  }

  onClose = (form: FormGroup) => {
    this.issueTrackerService.exportActive$.next(false);
    this.errorService.activeError$.next(null);
    this.setExportButtonFocus();
    if (form) {
      form.reset();
    }
  };

  paramTransform(param, value) {
    if (param == "impactRange") {
      return value;
    }
    if (!value) {
      return undefined;
    }
    return value;
  }

  onExport = () => {
    const issueTrackerReportObject: ExportReportParams = {};
    const issueTrackerAppParams = this.issueTrackerParamService
      .issueTrackerAppParams$.value;
    const issueTrackerParams = issueTrackerAppParams.issueTracker;

    issueTrackerReportObject.exportFormat = this.reportFormat;
    issueTrackerReportObject.fileName = this.reportName;
    issueTrackerReportObject.scanGroupId = issueTrackerParams.scanGroupId;
    issueTrackerReportObject.scanId = issueTrackerParams.scanId;
    issueTrackerReportObject.checkpointGroupId =
      issueTrackerParams.checkpointGroupId;

    if (
      this.issueTrackerExportsFileDownloadForm.controls["issuesType"].value ===
      "filteredIssues"
    ) {
      issueTrackerReportObject.severity = issueTrackerParams.severity;
      issueTrackerReportObject.impactRange = issueTrackerParams.impactRange;
      issueTrackerReportObject.priorityLevel = issueTrackerParams.priorityLevel;
      issueTrackerReportObject.checkpointId = issueTrackerParams.checkpointId;
      issueTrackerReportObject.state = issueTrackerParams.state;
    }

    const sanitizedParams = JSON.parse(
      JSON.stringify(issueTrackerReportObject, this.paramTransform)
    );

    this.issueTrackerService
      .exportIssueTrackerReport(sanitizedParams)
      .subscribe((res) => {
        if (res instanceof HttpResponse && res.status == 200) {
          // It is necessary to create a new blob object with mime-type explicitly set
          // otherwise only Chrome works like it should
          const newBlob = new Blob([res.body as Blob], {
            type: this.reportFormat,
          });

          // IE doesn't allow using a blob object directly as link href
          // instead it is necessary to use msSaveOrOpenBlob
          if (window.navigator && window.navigator.msSaveOrOpenBlob) {
            window.navigator.msSaveOrOpenBlob(newBlob);
            return;
          }

          // For other browsers:
          // Create a link pointing to the ObjectURL containing the blob.
          const data = window.URL.createObjectURL(newBlob);

          const link = document.createElement("a");
          link.href = data;
          link.download = this.reportName + "." + this.reportFormat;
          // this is necessary as link.click() does not work on the latest firefox
          link.dispatchEvent(
            new MouseEvent("click", {
              bubbles: true,
              cancelable: true,
              view: window,
            })
          );

          setTimeout(function () {
            // For Firefox it is necessary to delay revoking the ObjectURL
            window.URL.revokeObjectURL(data);
            link.remove();
          }, 100);
          this.onClose(this.issueTrackerExportsFileDownloadForm);
        }

        if (res.type == "ComplianceSheriffError") {
          const error = res;
          this.errorService.handleError(error);
        }
      });
  };
}
