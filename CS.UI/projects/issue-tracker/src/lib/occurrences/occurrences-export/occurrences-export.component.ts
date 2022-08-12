import { Component, HostListener, Input } from "@angular/core";
import { DatePipe } from "@angular/common";
import { FormBuilder, Validators, FormGroup } from "@angular/forms";
import { ESCAPE } from "@angular/cdk/keycodes";
import { HttpResponse } from "@angular/common/http";
import { tap } from "rxjs/operators";

import {
  ExportReportParams,
  IssueTrackerAppParams,
  OccurrenceParams,
} from "cs-core";
import { ErrorHandlerService, LoadingService } from "api-handler";
import { IssueTrackerParamService } from "../../issue-tracker-param.service";
import { OccurrencesService } from "../occurrences.service";
import { OccurrenceList } from "../../types";
import { InteractionsService } from "interactions";

@Component({
  selector: "app-occurrences-export",
  templateUrl: "./occurrences-export.component.html",
  styleUrls: ["./occurrences-export.component.scss"],
})
export class OccurrencesExportComponent {
  @Input()
  occurrences: OccurrenceList;
  @HostListener("keydown", ["$event"])
  onkeydown(event: KeyboardEvent) {
    if (event.keyCode === ESCAPE) {
      this.onClose(this.occurrencesExportsFileDownloadForm);
    }
  }
  constructor(
    private fb: FormBuilder,
    private errorService: ErrorHandlerService,
    private loadingService: LoadingService,
    private occurrencesService: OccurrencesService,
    private issueTrackerParamService: IssueTrackerParamService,
    private interactionsService: InteractionsService
  ) {
    this.issueTrackerAppParams = this.issueTrackerParamService.issueTrackerAppParams$.value;
    this.occurrenceParams = this.issueTrackerAppParams.occurrence;
  }
  issueTrackerAppParams: IssueTrackerAppParams;
  occurrenceParams: OccurrenceParams;
  isLoading = this.loadingService.loading$;
  activeError$ = this.errorService.activeError$;
  useCInvStyles = this.interactionsService.useCInvStyles;
  occurrencesExportsFileDownloadForm = this.fb.group({
    occurrencesType: [""],
    reportFormat: ["", Validators.required],
    reportName: ["", Validators.required],
  });
  reportFormat: string = "";
  reportName: string =
    "Occurrences_" + new DatePipe("en-US").transform(new Date(), "dd_MMM_yyyy");
  serverValidationError$ = this.errorService.serverValidationError$.pipe(
    tap((err) => {
      if (err) {
        this.occurrencesExportsFileDownloadForm.controls[err.field].setErrors({
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
    this.occurrencesService.occurrencesExportActive$.next(false);
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
    const occurrencesReportObject: ExportReportParams = {};
    occurrencesReportObject.exportFormat = this.reportFormat;
    occurrencesReportObject.fileName = this.reportName;
    occurrencesReportObject.issueId = this.occurrenceParams.issueId;
    occurrencesReportObject.scanGroupId = this.occurrenceParams.scanGroupId;
    occurrencesReportObject.scanId = this.occurrenceParams.scanId;
    occurrencesReportObject.checkpointGroupId = this.occurrenceParams.checkpointGroupId;
    occurrencesReportObject.checkpointId = this.occurrenceParams.checkpointId;

    if (
      this.occurrencesExportsFileDownloadForm.controls["occurrencesType"]
        .value === "filteredOccurrences"
    ) {
      occurrencesReportObject.pageTitle = this.occurrenceParams.pageTitle;
      occurrencesReportObject.pageUrl = this.occurrenceParams.pageUrl;
      occurrencesReportObject.element = this.occurrenceParams.element;
      occurrencesReportObject.keyAttribute = this.occurrenceParams.keyAttribute;
      occurrencesReportObject.containerId = this.occurrenceParams.containerId;
    }
    const sanitizedParams = JSON.parse(
      JSON.stringify(occurrencesReportObject, this.paramTransform)
    );
    this.occurrencesService
      .exportOccurrencesReport(sanitizedParams)
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
          this.onClose(this.occurrencesExportsFileDownloadForm);
        }
        if (res.type == "ComplianceSheriffError") {
          const error = res;
          this.errorService.handleError(error);
        }
      });
  };
}
