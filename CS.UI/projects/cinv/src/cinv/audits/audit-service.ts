import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, startWith, map, publishReplay, refCount } from 'rxjs/operators';
import { EMPTY, of, throwError, Observable } from 'rxjs';
import { BaseUrlResolver } from 'navigation';
import { AuditRequest } from './auditRequest';
import { APIResponse, makeObservableError, catchObservableError, ObservableError } from 'api-handler';
import { RecentAudit } from './recentAudit';
import { Audit } from './audit';
import { RecentAuditRequest } from './recentAuditRequest';
import { RecentAuditResponse } from './recentAuditResponse';

@Injectable({
  providedIn: 'root'
})
export class AuditService {

  createAuditUrl = `rest/Scan/create`;
  runAuditUrl = `rest/Scan/run`;
  recentAuditsUrl = `rest/Scan/recentScans`;
  auditByNameUrl = `rest/Scan/getByName`;
  auditStatusUrl = `rest/Scan/runStatus`;
  updateAuditUrl = `rest/Scan/update`;
  deleteAuditUrl = `rest/Scan/delete`;

  constructor(private http: HttpClient,
              private baseUrlResolver: BaseUrlResolver) { }

  createAudit(auditRequest: AuditRequest) : Observable<APIResponse | ObservableError>
  {
      return this.http.post<any>(this.createAuditUrl, auditRequest)
      .pipe(
        startWith(0),
        map((response) => response as APIResponse),        
        catchError((response) => {
          if (response instanceof HttpErrorResponse) {
            return of(
              makeObservableError(
                response,
                "api:user:Create:apiError"
              )
            );
          } else {
            return throwError(response);
          }
        }),
        catchObservableError("api:Create:user:apiError"),
        publishReplay(1),
        refCount(),
      );
  }

  deleteAudit(scanId: string)
  {
    return this.http.delete<any>(this.deleteAuditUrl + '/' + scanId)
    .pipe(
      startWith(0),
      map((response) => response as APIResponse),        
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(
            makeObservableError(
              response,
              "api:user:Create:apiError"
            )
          );
        } else {
          return throwError(response);
        }
      }),
      catchObservableError("api:Create:user:apiError"),
      publishReplay(1),
      refCount(),
    );    
  }  

  runAudit(auditId: string) 
  {
    return this.http.post<any>(this.runAuditUrl, { "scanId" : auditId })
    .pipe(
      startWith(0),
      map((response) => response as APIResponse),        
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(
            makeObservableError(
              response,
              `api:scan:run:${response.statusText}`
            )
          );
        } else {
          return throwError(response);
        }
      }),
      catchObservableError("api:scan:run:apiError"),
      publishReplay(1),
      refCount(),
    );
  }

  getRecentAudits(recentAuditRequest: RecentAuditRequest) : Observable<RecentAuditResponse | ObservableError>
  {
      return this.http.post(this.recentAuditsUrl, recentAuditRequest)
      .pipe(
        startWith(0),
        map((response) => response as RecentAuditResponse),        
        catchError((response) => {
          if (response instanceof HttpErrorResponse && response.status === 409 || response.status === 401) {
            return of(
              makeObservableError(
                response,
                "api:user:Create:apiError"
              )
            );
          } else {
            return throwError(response);
          }
        }),
        catchObservableError("api:Create:user:apiError"),
        publishReplay(1),
        refCount(),
      );
  }
  
  getAuditByName(auditName: string) : Observable<Audit | ObservableError>
  {
      return this.http.get(this.auditByNameUrl + "/" + auditName)
      .pipe(
        startWith(0),
        map((response) => response as Audit),        
        catchError((response) => {
          if (response instanceof HttpErrorResponse && response.status === 409 || response.status === 401) {
            return of(
              makeObservableError(
                response,
                "api:user:Create:apiError"
              )
            );
          } else {
            return throwError(response);
          }
        }),
        catchObservableError("api:Create:user:apiError"),
        publishReplay(1),
        refCount(),
      );
  }
  
  getAuditStatus(auditId: number) : Observable<APIResponse | ObservableError>
  {
      return this.http.get(this.auditStatusUrl + "/" + auditId)
      .pipe(
        startWith(0),
        map((response) => response as APIResponse),        
        catchError((response) => {
          if (response instanceof HttpErrorResponse) {
            return of(
              makeObservableError(
                response,
                "api:user:Create:apiError"
              )
            );
          } else {
            return throwError(response);
          }
        }),
        catchObservableError("api:Create:user:apiError"),
        publishReplay(1),
        refCount(),
      );
  }

  updateAudit(audit: AuditRequest) : Observable<APIResponse | ObservableError | any>
  {
    return this.http.post<any>(this.updateAuditUrl, audit)
    .pipe(
      startWith(0),
      map((response) => response),        
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(
            makeObservableError(
              response,
              "api:scan:update:apiError"
            )
          );
        } else {
          return throwError(response);
        }
      }),
      catchObservableError("api:scan:update:apiError"),
      publishReplay(1),
      refCount(),
    );
  }  
}
