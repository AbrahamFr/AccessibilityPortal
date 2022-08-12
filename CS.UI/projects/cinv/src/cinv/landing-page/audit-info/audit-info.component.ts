import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { EMPTY, Observable, of } from "rxjs";
import { map, mergeMap } from "rxjs/operators";
import { CreateUserParams } from "cs-core";
import { FormBuilder, Validators } from "@angular/forms";

import { InteractionsService } from "interactions";
import { OrganizationIdService } from "navigation";
import {
  ErrorHandlerService,
  UserService,
  ObservableError,
  isObservableError,
  APIResponse,
} from "api-handler";
import {
  User,
  AuthRequest,
  AuthenticationService,
  JwtStoreService,
  LoginService,
} from "authentication";
import { AuditService } from "../../audits/audit-service";
import { AuditRequest } from "../../audits/auditRequest";
import { ApiErrorTranslatorService } from "../../shared/api-error-translator.service";
import { AuditMySiteProcessorService } from "../../shared/audit-my-site-processor.service";
import { ValidatorService } from "../../shared/validator.service";

@Component({
  selector: "cinv-audit-info",
  templateUrl: "./audit-info.component.html",
  styleUrls: ["./audit-info.component.scss"],
})
export class AuditInfoComponent implements OnInit {
  private selectedguidelines: string[] = [];
  isSubmitted: boolean = false;

  user: Observable<User>;
  private clientSettings: any;
  public errorMsg: string;

  constructor(
    private userService: UserService,
    private fb: FormBuilder,
    private jwtStore: JwtStoreService,
    private loginService: LoginService,
    private authService: AuthenticationService,
    private auditService: AuditService,
    private router: Router,
    private httpClient: HttpClient,
    private validator: ValidatorService,
    private orgIdService: OrganizationIdService,
    private apitranslator: ApiErrorTranslatorService,
    private auditMySiteProcessorService: AuditMySiteProcessorService,
    private errorService: ErrorHandlerService,
    private interactionsService: InteractionsService
  ) {}

  ngOnInit(): void {
    this.httpClient.get("assets/clientSettings.json").subscribe((data) => {
      this.clientSettings = data;
    });
    this.loginService.logout();
    this.interactionsService.useCInvStyles = true;
  }

  get f() {
    return this.createUserForm.controls;
  }

  createUserForm = this.fb.group({
    userName: [
      "",
      [
        Validators.required,
        Validators.pattern(new RegExp(this.validator.emailRegex)),
      ],
    ],
    firstName: [
      "",
      [
        Validators.required,
        Validators.pattern(new RegExp(this.validator.nameRegex)),
      ],
    ],
    lastName: [
      "",
      [
        Validators.required,
        Validators.pattern(new RegExp(this.validator.nameRegex)),
      ],
    ],
    password: [
      "",
      [
        Validators.required,
        Validators.pattern(new RegExp(this.validator.passwordRegex)),
      ],
    ],
    emailAddress: [""],
    url: [
      "",
      [
        Validators.required,
        Validators.pattern(new RegExp(this.validator.htmlRegex)),
      ],
    ],
    guideline: ["", Validators.required],
  });

  onSubmit() {
    this.isSubmitted = true;

    if (this.createUserForm.invalid) {
      return;
    }

    this.orgIdService.useOrgVirtualDir = false;

    const newUser: CreateUserParams = {
      firstName: this.createUserForm.controls["firstName"].value,
      lastName: this.createUserForm.controls["lastName"].value,
      userName: this.createUserForm.controls["userName"].value,
      passWord: this.createUserForm.controls["password"].value,
      organizationId: this.clientSettings.organizationId,
      userGroupName: this.createUserForm.controls["userName"].value,
      emailAddress: this.createUserForm.controls["userName"].value,
    };

    this.userService
      .createUser(newUser)
      .pipe(
        map((res) => {
          if (res) {
            if (isObservableError(res)) {
              this.errorMsg = this.apitranslator.translateErrorMessage(
                (res as ObservableError).code
              );

              return EMPTY;
            }
            const newAuthRequest: AuthRequest = {
              UserName: newUser.userName!,
              Password: newUser.passWord!,
              OrganizationId: newUser.organizationId!,
              AuthenticationType: this.clientSettings.authenticationType,
            };

            const jwtClaims = this.loginService
              .loginWith(newAuthRequest)
              .subscribe((data) => {
                if (newUser.userName === this.loginService.getUserName()) {
                  // Set properties for new Audit to be created
                  this.auditMySiteProcessorService.startingUrl = this.createUserForm.controls[
                    "url"
                  ].value;
                  this.auditMySiteProcessorService.guideline = this.createUserForm.controls[
                    "guideline"
                  ].value;

                  //navigate to processing page
                  this.router.navigate(["/processing"]);
                }
              });
          }
        })
      )
      .subscribe();
  }

  getScanCall() {
    const auditRequest: AuditRequest = {
      baseUrl: this.createUserForm.controls["url"].value,
      checkpointGroupIds: this.selectedguidelines,
      displayName: this.createUserForm.controls["url"].value,
      path: "/",
      levels: "5",
      pageLimit: "5",
    };

    return this.auditService.createAudit(auditRequest).pipe(
      mergeMap((res: APIResponse) => {
        if (res && res.data) {
          this.jwtStore.receiveJwtToken(null);

          const jwtClaims = this.authService.jwtClaims$
            .pipe(
              map((val) => {
                //run new audit
                return this.auditService
                  .runAudit(res.data["scanId"])
                  .subscribe();
              })
            )
            .subscribe();
        }
        return of(res);
      })
    );
  }
}
