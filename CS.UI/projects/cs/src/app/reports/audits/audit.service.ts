import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpResponse,
  HttpErrorResponse,
} from "@angular/common/http";
import {
  map,
  publishReplay,
  startWith,
  refCount,
  catchError,
} from "rxjs/operators";
import { AuditReport, AuditReportFormData } from "../../data-types/types";
import { Observable, BehaviorSubject, of, throwError } from "rxjs";
import {
  catchObservableError,
  ObservableError,
  makeObservableError,
} from "api-handler";

@Injectable({
  providedIn: "root",
})
export class AuditService {
  readonly refreshList$ = new BehaviorSubject<boolean>(false);
  private auditReportsUrl = "rest/AuditReport/AuditReportsList";
  private auditReportUploadUrl = "rest/AuditReport/upload";
  private auditReportEditUrl = "rest/AuditReport/edit";

  constructor(private http: HttpClient) {}

  getAuditReportsList(): Observable<AuditReport[] | ObservableError> {
    return this.http.get(this.auditReportsUrl).pipe(
      startWith(0),
      map((response) => response as AuditReport[]),
      catchError((response) => {
        if (response instanceof HttpErrorResponse && response.status === 403) {
          return of(
            makeObservableError(
              response,
              "api:auditReport:auditreportslist:createOnly"
            )
          );
        } else {
          return throwError(response);
        }
      }),
      catchObservableError("api:auditReport:apiError"),
      publishReplay(1),
      refCount()
    );
  }

  uploadAuditReport(
    fileToUpload: File | any,
    data: AuditReportFormData
  ): Observable<HttpResponse<any> | ObservableError> {
    const fileName = fileToUpload.name;
    const urlWithFileName = `${this.auditReportUploadUrl}?fileName=${fileName}`;
    const formData: FormData = new FormData();

    formData.append("reportName", data.reportName);
    formData.append("reportType", data.reportType);
    formData.append("reportDescription", data.reportDescription);
    formData.append("FileToUpload", fileToUpload);

    return this.http
      .post(urlWithFileName, formData, { observe: "response" })
      .pipe(
        map((response) => response as HttpResponse<any>),
        catchObservableError("api:auditReport:apiError")
      );
  }

  editAuditReportData(
    auditReport: AuditReport
  ): Observable<HttpResponse<any> | ObservableError> {
    return this.http
      .put(this.auditReportEditUrl, auditReport, { observe: "response" })
      .pipe(
        map((response) => response as HttpResponse<any>),
        catchObservableError("api:auditReport:apiError")
      );
  }

  deleteAuditReport(
    auditReportId: number
  ): Observable<HttpResponse<any> | ObservableError> {
    const deleteAuditReportUrl = `rest/AuditReport/delete/${auditReportId}`;

    return this.http.delete(deleteAuditReportUrl, { observe: "response" }).pipe(
      map((response) => response as HttpResponse<any>),
      catchObservableError("api:auditReport:apiError")
    );
  }

  downloadAuditReport(
    auditReportId: number
  ): Observable<HttpResponse<any> | ObservableError> {
    const downloadAuditReportUrl = `rest/AuditReport/download/${auditReportId}`;
    return this.http
      .get(downloadAuditReportUrl, {
        responseType: "blob",
        observe: "response",
      })
      .pipe(
        map((response) => response as HttpResponse<any>),
        catchObservableError("api:auditReport:apiError")
      );
  }
}
