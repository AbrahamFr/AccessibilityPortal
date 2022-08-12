import { Injectable, OnInit } from "@angular/core";
import { HttpErrorResponse } from "@angular/common/http";
import {
  APIResponse,
  makeObservableError,
  catchObservableError,
  isObservableError,
} from "api-handler";
import {
  map,
  mergeMap,
  catchError,
  concatMap,
  retry,
  share,
  takeUntil,
} from "rxjs/operators";
import { Router } from "@angular/router";
import { of } from "rxjs/internal/observable/of";
import { AuditService } from "../audits/audit-service";
import { AuditRequest } from "../audits/auditRequest";
import { GuidelinesService } from "../guidelines/guidelines.service";
import { throwError, Observable, timer, Subject } from "rxjs";
import { UrlParamsService } from 'navigation';
import { UrlParserService } from './url-parser.service';

@Injectable({
  providedIn: "root",
})
export class AuditMySiteProcessorService implements OnInit {
  constructor(
    private router: Router,
    private auditService: AuditService,
    private urlParamsService: UrlParamsService,
    private guidelinesService: GuidelinesService,
    private urlParserService: UrlParserService
  ) {}

  private _startingUrl: string;
  private _guideline: string;
  private _runId: number;
  public stopPolling = new Subject();

  get startingUrl(): string {
    return this._startingUrl;
  }

  set startingUrl(value: string) {
    this._startingUrl = value;
  }

  get guideline(): string {
    return this._guideline;
  }

  set guideline(value: string) {
    this._guideline = value;
  }

  get runId(): number {
    return this._runId;
  }

  ngOnInit(): void {}

  getAuditCall() {
    const auditRequest: AuditRequest = {
      baseUrl: this.urlParserService.parseUrl(this._startingUrl, true),
      checkpointGroupIds: this.guidelinesService.calculateGuidelines(
        this._guideline
      ),
      displayName: this.urlParserService.parseUrl(this._startingUrl, true) + " (My First Audit)",
      path: this.urlParserService.parseUrl(this._startingUrl),
      levels: "5",
      pageLimit: "5",
    };

    return this.auditService.createAudit(auditRequest).pipe(
      mergeMap((res: APIResponse) => {
        return of(res);
      }),
      catchError((error) => {
        return throwError(error.error.message);
      })
    );
  }

  pollAuditStatus(scanId: number) {
    const timer$ = timer(1, 3000).pipe(
      concatMap(() =>
        this.getAuditStatus(scanId).pipe(
          map((response: APIResponse) => {
            if (response) {
              let auditStatus: number = (response as APIResponse).data["runId"][
                "runStatusValue"
              ];

              if (auditStatus === 3)
              {
                this.stopPolling.next(auditStatus);
                return response;
              }

              if (auditStatus !== 4 && auditStatus !== 1) {
                this.stopPolling.next();

                const issueTrackerAppParams  = {
                  issueTracker : {
                    currentPage: 1,
                    recordsToReturn: 20,
                    checkpointGroupId: "",
                    summaryFilters: {},
                    scanId: scanId
                  },
              
                  occurrence : {
                    currentPage: 1,
                    recordsToReturn: 20,
                    checkpointGroupId: "",
                    scanId: scanId
                  }
                }

                this.router.navigate(["/auditresults"], {
                  queryParams: { scanId: scanId, data: this.urlParamsService.encodeAppData(issueTrackerAppParams) }
                });
              }

              return response;
            }
          }),
          catchError((response) => {
            if (response instanceof HttpErrorResponse) {
              return of(makeObservableError(response, response.error));
            } else {
              return throwError(response);
            }
          })
        )
      )
    );

    return timer$.pipe(retry(), share(), takeUntil(this.stopPolling));
  }

  getAuditStatus(scanId: number): Observable<any> {
    return this.auditService.getAuditStatus(scanId).pipe(
      map((response) => {
        return response;
      }),
      catchError((response) => {
        if (response instanceof HttpErrorResponse) {
          return of(makeObservableError(response, response.error));
        } else {
          return throwError(response);
        }
      })
    );
  }
}
