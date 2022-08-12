import {
  Component,
  OnInit,
  ElementRef,
  Input,
  ViewChildren,
} from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { AuditService } from "../audit.service";
import { HttpResponse } from "@angular/common/http";
import { AuditReport } from "../../../data-types/types";
import {
  PreviousActiveElementService,
  NextActiveElementService,
  SetNextElementRefService,
} from "interactions";
import { ObservableError } from "api-handler";

@Component({
  selector: "app-audit-report-list",
  templateUrl: "./audit-report-list.component.html",
  styleUrls: ["./audit-report-list.component.scss"],
})
export class AuditReportListComponent implements OnInit {
  readonly selectedReport$ = new BehaviorSubject<AuditReport | null>(null);
  @ViewChildren("auditReportsListItems", { read: ElementRef })
  auditReportsListItems;
  @Input()
  auditReports: AuditReport[];

  constructor(
    private auditService: AuditService,
    private activeElementService: PreviousActiveElementService,
    private nextActiveElementService: NextActiveElementService,
    private setNextElementRefService: SetNextElementRefService
  ) {}

  previousActiveElement$ = this.activeElementService.previousActiveElement$;
  nextActiveElement$ = this.nextActiveElementService.nextActiveElement$;

  isDeleteActive: boolean = false;
  readonly activeDelete$ = new BehaviorSubject<boolean>(this.isDeleteActive);

  hasDownloadError: boolean = false;
  readonly activeDownloadError$ = new BehaviorSubject<ObservableError | null>(
    null
  );

  readonly activeDownload$ = new BehaviorSubject<boolean>(false);

  isEditActive: boolean = false;
  readonly activeEdit$ = new BehaviorSubject<boolean>(this.isEditActive);

  reportTypes: string[] = ["Baseline Audit", "Regression", "VPAT", "Other"];

  displayedColumns$ = new BehaviorSubject<string[]>([
    "reportName",
    "reportDescription",
    "auditTypeName",
    "fileUploadDate",
    "fileSize",
    "fileType",
    "actions",
  ]);

  ngOnInit() {}

  onClickEditReport(auditReport: AuditReport) {
    this.nextActiveElementService.nextActiveElement$.next(null);
    this.activeElementService.previousActiveElement$.next(
      document.activeElement as HTMLElement
    );
    this.isEditActive = !this.isEditActive;
    this.activeEdit$.next(this.isEditActive);
    this.selectedReport$.next(auditReport);
  }

  onClickDeleteRow(auditReport: AuditReport) {
    this.setNextElementRefService.setNextElementRef$.next(null);
    this.handleSetFocusOnDeleteRow(auditReport.auditReportId);
    this.activeElementService.previousActiveElement$.next(
      document.activeElement as HTMLElement
    );
    this.isDeleteActive = !this.isDeleteActive;
    this.activeDelete$.next(this.isDeleteActive);
    this.selectedReport$.next(auditReport);
  }

  handleSetFocusOnDeleteRow(reportId: number) {
    const reportsListElements = this.auditReportsListItems.toArray();
    const activeRow = reportsListElements.find(
      (el: ElementRef) => el.nativeElement.id == reportId
    );
    const rowIndex = reportsListElements.findIndex(
      (el: ElementRef) => el.nativeElement.id == reportId
    );

    if (activeRow && activeRow.nativeElement.nextSibling) {
      this.setNextElementRefService.setNextElementRef$.next(
        reportsListElements[rowIndex + 1].nativeElement
      );
    }

    if (
      activeRow &&
      !activeRow.nativeElement.nextSibling &&
      reportsListElements.length > 1
    ) {
      this.setNextElementRefService.setNextElementRef$.next(
        reportsListElements[rowIndex - 1].nativeElement
      );
    }

    if (activeRow && reportsListElements.length === 1) {
      const addBtnRef = document.getElementById("add-report-btn");
      this.setNextElementRefService.setNextElementRef$.next(addBtnRef);
    }
  }

  onClickDownloadAuditReport(auditReport: AuditReport) {
    this.nextActiveElementService.nextActiveElement$.next(null);
    this.activeElementService.previousActiveElement$.next(
      document.activeElement as HTMLElement
    );
    this.activeDownload$.next(true);
    this.selectedReport$.next(auditReport);
    this.auditService
      .downloadAuditReport(auditReport.auditReportId)
      .subscribe((res) => {
        if (res instanceof HttpResponse) {
          // It is necessary to create a new blob object with mime-type explicitly set
          // otherwise only Chrome works like it should
          const newBlob = new Blob([res.body as Blob], {
            type: auditReport.fileType,
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
          const fileLocation = auditReport.fileLocation;
          const fileNameToBeDownloaded = fileLocation.replace(/^.*[\\\/]/, "");
          link.download = fileNameToBeDownloaded;
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
          this.activeDownload$.next(false);
        }

        if (res.type == "ComplianceSheriffError") {
          const error = res;
          this.hasDownloadError = !this.hasDownloadError;
          this.activeDownload$.next(false);
          this.activeDownloadError$.next(error);
        }
      });
  }

  onCloseDelete() {
    this.isDeleteActive = !this.isDeleteActive;
    this.activeDelete$.next(false);
    this.activeElementService.setFocus();
  }

  onCloseDownloadError() {
    this.hasDownloadError = !this.hasDownloadError;
    this.activeDownloadError$.next(null);
    this.activeElementService.setFocus();
  }

  onCloseEdit() {
    this.isEditActive = !this.isEditActive;
    this.activeEdit$.next(false);
    this.activeElementService.setFocus();
  }
}
