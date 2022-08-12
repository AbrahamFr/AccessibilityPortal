import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  ViewChild,
} from "@angular/core";
import { FormBuilder, Validators, FormGroup } from "@angular/forms";
import { EMPTY } from "rxjs";
import { mergeMap, map, catchError } from "rxjs/operators";
import { of } from "rxjs/internal/observable/of";

import {
  isObservableError,
  makeObservableError,
  ScanGroupsScansService,
} from "api-handler";
import { AuthenticationService, JwtStoreService } from "authentication";
import { AuditService } from "../../audits/audit-service";
import { AuditRequest } from "../../audits/auditRequest";
import { GuidelinesService } from "../../guidelines/guidelines.service";
import { ApiErrorTranslatorService } from "../../shared/api-error-translator.service";
import { ValidatorService } from "../../shared/validator.service";
import { Audit } from "../../audits/audit";
import { AddScanGroupScansParams } from "cs-core";
import { ScanGroupsService } from "api-handler";
import { UrlParserService } from '../../shared/url-parser.service';

@Component({
  selector: "cinv-audit-form",
  templateUrl: "./audit-form.component.html",
  styleUrls: ["./audit-form.component.scss"],
})
export class AuditFormComponent implements OnInit, OnChanges {
  @ViewChild("auditName") auditName: ElementRef;
  @Input() selectedAudit: Audit | undefined;
  @Output() refreshData = new EventEmitter<any>();
  @Output() changeView = new EventEmitter<{
    pageView: string;
    rowData: object | null;
  }>();  

  formHeader = "Start New Audit";

  private selectedguidelines: string[] = [];
  auditForm: FormGroup;
  isSubmitted: boolean = false;
  apiErrorResponse: string;
  levelNumber: number = 0;
  pageLimitNumber: number = 5;

  constructor(
    private fb: FormBuilder,
    private jwtStore: JwtStoreService,
    public auditService: AuditService,
    private apiErrorTranslatorService: ApiErrorTranslatorService,
    private authService: AuthenticationService,
    private guidelinesService: GuidelinesService,
    private validatorService: ValidatorService,
    private scanGroupsService: ScanGroupsService,
    private scanGroupScansService: ScanGroupsScansService,
    private urlParserService: UrlParserService
  ) {}

  get f() {
    return this.auditForm.controls;
  }

  ngOnInit(): void {
    this.auditForm = this.fb.group({
      auditName: ["", Validators.required],
      guideline: ["", Validators.required],
      startPage: [
        "",
        [
          Validators.required,
          Validators.pattern(new RegExp(this.validatorService.htmlRegex)),
        ],
      ],
      pageLimit: [
        "",
        [Validators.required, Validators.max(25), Validators.min(1)],
      ],
      levels: ["", [Validators.required, Validators.min(0), Validators.max(5)]],
    });

    this.auditForm.patchValue({ pageLimit: this.pageLimitNumber });
    this.auditForm.patchValue({ levels: this.levelNumber });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (this.selectedAudit) {
      this.formHeader = "Edit Audit: ";

      this.auditForm.patchValue({ auditName: this.selectedAudit.scanName });
      this.auditForm.patchValue({
        guideline: this.selectedAudit.checkpointGroupId,
      });
      this.auditForm.patchValue({
        startPage: this.selectedAudit.startingUrl,
      });
      this.auditForm.patchValue({ pageLimit: this.selectedAudit.pageLimit });
      this.auditForm.patchValue({ levels: this.selectedAudit.levels });

      this.apiErrorResponse = "";

      this.auditName.nativeElement.focus();
    }
  }

  levelsChange(value: number) {
    this.levelNumber = value;
  }

  pageLimitChange(value: number) {
    this.pageLimitNumber = value;
  }

  clearform() {
    this.isSubmitted = false;
    this.auditForm.reset();
    this.auditForm.markAsPristine();
    this.auditForm.patchValue({ pageLimit: this.pageLimitNumber });
    this.auditForm.patchValue({ levels: this.levelNumber });
    this.selectedAudit = undefined;
    this.formHeader = "Start New Audit";
    this.apiErrorResponse = "";
    this.refreshData.emit("true");
    this.changeView.emit({ pageView: "Start", rowData: null });
  }

  addAuditToScanGroup(scanId: number) {
    const addScanGroupScansParams: AddScanGroupScansParams = {
      scanGroupId: this.scanGroupsService.getScanGroupId(),
      scanList: [scanId],
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
  }

  copyAudit() {
    this.isSubmitted = true;

    if (this.auditForm.invalid) {
      return;
    }

    if (this.selectedguidelines.length === 0) {
      this.calculateGuidelines(this.f["guideline"].value);
    }

    const auditRequest: AuditRequest = {
      baseUrl: this.urlParserService.parseUrl(this.f["startPage"].value, true),
      displayName: this.f["auditName"].value + " (copy)",
      checkpointGroupIds: this.selectedguidelines,
      pageLimit: this.f["pageLimit"].value,
      path: this.urlParserService.parseUrl(this.f["startPage"].value),
      levels: this.f["levels"].value,
    };

    return this.auditService
      .createAudit(auditRequest)
      .pipe(
        map((res: any) => {
          if (isObservableError(res)) {
            this.apiErrorResponse = this.apiErrorTranslatorService.translateErrorMessage(
              res.error.error.errorCode
            );

            return EMPTY;
          }

          if (res && res.data) {
            //add audit to scanGroup
            this.addAuditToScanGroup(res.data["scanId"]);

            this.clearform();
          }
        })
      )
      .subscribe();
  }

  saveAudit(runAudit: boolean = false) {
    this.isSubmitted = true;

    if (this.auditForm.invalid) {
      return;
    }

    if (this.selectedguidelines.length === 0) {
      this.calculateGuidelines(this.f["guideline"].value);
    }

    if (!this.selectedAudit) {
      const auditRequest: AuditRequest = {
        baseUrl: this.urlParserService.parseUrl(this.f["startPage"].value, true),
        displayName: this.f["auditName"].value,
        checkpointGroupIds: this.selectedguidelines,
        pageLimit: this.f["pageLimit"].value,
        path: this.urlParserService.parseUrl(this.f["startPage"].value),
        levels: this.f["levels"].value,
      };

      return this.auditService
        .createAudit(auditRequest)
        .pipe(
          mergeMap((res: any) => {
            if (isObservableError(res)) {
              this.apiErrorResponse = this.apiErrorTranslatorService.translateErrorMessage(
                res.code
              );
              return EMPTY;
            }
            if (res && res.data) {
              //add audit to scanGroup
              this.addAuditToScanGroup(res.data["scanId"]);
              this.jwtStore.receiveJwtToken(null);

              const jwtClaims = this.authService.jwtClaims$
                .pipe(
                  map((val) => {
                    if (runAudit) {
                      //run new audit
                      return this.runAudit(res.data["scanId"]);
                    } else {
                      this.clearform();
                    }
                  })
                )
                .subscribe();
            }
            return of(res);
          }),
          catchError((response) => {
            return EMPTY;
          })
        )
        .subscribe();
    } else {
      const auditRequest: AuditRequest = {
        baseUrl: this.urlParserService.parseUrl(this.f["startPage"].value, true),
        displayName: this.f["auditName"].value,
        checkpointGroupIds: this.selectedguidelines,
        pageLimit: this.f["pageLimit"].value,
        path: this.urlParserService.parseUrl(this.f["startPage"].value),
        levels: this.f["levels"].value,
        scanId: this.selectedAudit.scanId.toString(),
      };

      return this.auditService
        .updateAudit(auditRequest)
        .pipe(
          map((response: any) => {
            if (response) {
              if (isObservableError(response)) {
                this.apiErrorResponse = this.apiErrorTranslatorService.translateErrorMessage(
                  response.code
                );

                return makeObservableError(
                  this.apiErrorResponse,
                  response.code
                );
              }

              if (runAudit) {
                this.runAudit(this.selectedAudit!.scanId.toString());
                return EMPTY;
              } else {
                this.clearform();
              }
            }
          })
        )
        .subscribe();
    }
  }

  private runAudit(scanId: string) {
    return this.auditService
      .runAudit(scanId)
      .pipe(
        map((response) => {
          if (response) {
            if (isObservableError(response)) {
              this.apiErrorResponse = this.apiErrorTranslatorService.translateErrorMessage(
                response.code
              );
              return makeObservableError(this.apiErrorResponse, response.code);
            }

            this.clearform();
            return EMPTY;
          }
        })
      )
      .subscribe();
  }

  calculateGuidelines(selection: string) {
    this.selectedguidelines.length = 0;
    this.selectedguidelines = this.guidelinesService.calculateGuidelines(
      selection
    );
  }
}
