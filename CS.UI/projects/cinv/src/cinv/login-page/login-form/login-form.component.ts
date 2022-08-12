import { Component, OnInit } from "@angular/core";
import { FormBuilder, Validators } from "@angular/forms";
import { HttpClient } from "@angular/common/http";
import { Router, ActivatedRoute } from "@angular/router";
import { EMPTY, Observable } from "rxjs";
import { catchError, map } from "rxjs/operators";

import { InteractionsService } from "interactions";
import { OrganizationIdService } from "navigation";
import {
  AuthRequest,
  LoginService,
  LoginMessagingService,
} from "authentication";
import { ApiErrorTranslatorService } from "../../shared/api-error-translator.service";
import { ValidatorService } from "../../shared/validator.service";

@Component({
  selector: "cinv-login-form",
  templateUrl: "./login-form.component.html",
  styleUrls: ["./login-form.component.scss"],
})
export class LoginFormComponent implements OnInit {
  isSubmitted: boolean = false;
  private clientSettings: any;
  returnUrl: string;
  clientMessages: Observable<string | null>;

  jwtClaims: Observable<Record<string, any> | null>;

  loginForm = this.fb.group({
    userName: ["", [Validators.required, Validators.pattern(this.validator.emailRegex)]],
    password: ["", [Validators.required, Validators.pattern(this.validator.passwordRegex)]],
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private httpClient: HttpClient,
    private loginService: LoginService,
    private validator: ValidatorService,
    private orgIdService: OrganizationIdService,
    private loginMessagingService: LoginMessagingService,
    private apiTranslatorService: ApiErrorTranslatorService,
    private interactionsService: InteractionsService
  ) {}

  ngOnInit(): void {
    this.httpClient.get("assets/clientSettings.json").subscribe((data) => {
      this.clientSettings = data;
    });

    // get return url from route parameters or default to '/myhome'
    this.returnUrl = this.route.snapshot.queryParams["returnUrl"] || "/myhome";
    this.loginService.logout();
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit() {
    this.isSubmitted = true;

    if (this.loginForm.invalid) {
      return;
    }

    this.interactionsService.useCInvStyles = true;
    this.orgIdService.useOrgVirtualDir = false;

    this.clientMessages = this.loginMessagingService.loginErrors$.pipe(
      map((response) => {
        if (response != null) {
          return this.apiTranslatorService.translateErrorMessage(response);
        }
        return null;
      })
    );

    //Call Authenticate and pass in public user
    const authRequest: AuthRequest = {
      UserName: this.loginForm.controls["userName"].value,
      Password: this.loginForm.controls["password"].value,
      OrganizationId: this.clientSettings.organizationId,
      AuthenticationType: this.clientSettings.authenticationType,
    };
    this.loginService
      .loginWith(authRequest)
      .pipe(
        catchError((response) => {
          return EMPTY;
        })
      )
      .subscribe(
        (data) => {
          this.router.navigate([this.returnUrl]);
        },
        (error) => console.log(`error: ${error}`)
      );
  }
}
