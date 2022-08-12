import { Component, OnInit } from "@angular/core";
import { AuditMySiteProcessorService } from "../shared/audit-my-site-processor.service";
import { map } from "rxjs/operators";
import { isObservableError, ScanGroupsService } from "api-handler";
import { ApiErrorTranslatorService } from "../shared/api-error-translator.service";
import { Router } from "@angular/router";
import { of } from "rxjs";
import { RefreshTokenService } from "authentication";
import { AuditService } from "../audits/audit-service";
import { AddScanGroupScansParams, CreateScanGroupParams } from "cs-core";
import { ScanGroupsScansService } from "api-handler";

@Component({
  selector: "cinv-audit-processing-page",
  templateUrl: "./audit-processing-page.component.html",
  styleUrls: ["./audit-processing-page.component.scss"],
})
export class AuditProcessingPageComponent implements OnInit {
  public errorMsg: string;
  private scanGroupId: number;

  constructor(
    private auditMySiteProcessorService: AuditMySiteProcessorService,
    private apitranslator: ApiErrorTranslatorService,
    private router: Router,
    private refreshTokenService: RefreshTokenService,
    private auditService: AuditService,
    private scanGroupsService: ScanGroupsService,
    private scanGroupScansService: ScanGroupsScansService
  ) {}

  ngOnInit(): void {
    const createScanGroupParams: CreateScanGroupParams = {
      scanGroupName: localStorage.getItem("userName")!,
      setAsDefault: true,
    };

    //create scanGroup
    this.scanGroupsService
      .createScanGroup(createScanGroupParams)
      .pipe(
        map((response) => {
          if (response) {
            if (isObservableError(response)) {
              this.errorMsg = this.apitranslator.translateErrorMessage(
                response.error.error.errorCode
              );
              return of(response);
            }

            this.scanGroupId = response.data["scanGroupId"];
          }
        })
      )
      .subscribe();

    //create new audit, run audit, navigate to homepage
    const createScan = this.auditMySiteProcessorService.getAuditCall();
    createScan
      .pipe(
        map((response) => {
          if (response) {
            if (isObservableError(response)) {
              this.errorMsg = this.apitranslator.translateErrorMessage(
                response.error.error.errorCode
              );
              return of(response);
            }

            const addScanGroupScansParams: AddScanGroupScansParams = {
              scanGroupId: this.scanGroupId,
              scanList: [response.data["scanId"]],
            };
            //Associate Scan with ScanGroup
            this.scanGroupScansService
              .addScanGroupScans(addScanGroupScansParams)
              .pipe(
                map((response) => {
                  if (response) {
                    //Swallow error response at this time
                    if (isObservableError(response)) {
                      return of(response);
                    }
                  }
                })
              )
              .subscribe();

            this.refreshTokenService
              .refreshToken()
              .pipe(
                map((res) => {
                  //run new audit
                  this.auditService
                    .runAudit(response.data["scanId"])
                    .pipe(
                      map((res) => {
                        if (res) {
                          if (isObservableError(res)) {
                            this.errorMsg = this.apitranslator.translateErrorMessage(
                              res.code
                            );
                            return of(res);
                          }

                          this.pollAuditStatus(response.data["scanId"]);

                          this.auditMySiteProcessorService.stopPolling
                            .pipe(
                              map((response) => {
                                if (response === 3) {
                                  (this.errorMsg = this.apitranslator.translateErrorMessage(
                                    "api:scan:runStatus:aborted"
                                  )),
                                    "\n Please click button below to the My Home page";
                                }
                              })
                            )
                            .subscribe();
                        }
                      })
                    )
                    .subscribe();
                })
              )
              .subscribe((data) => {});
          }
        })
      )
      .subscribe();
  }

  pollAuditStatus(scanId: number) {
    const polling$ = this.auditMySiteProcessorService
      .pollAuditStatus(scanId)
      .subscribe();
  }
}
