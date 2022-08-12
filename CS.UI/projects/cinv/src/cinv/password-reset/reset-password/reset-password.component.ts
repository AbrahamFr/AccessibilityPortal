import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import {
  AbstractControl,
  FormBuilder,
  ValidationErrors,
  Validators,
} from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { isObservableError, UserService } from "api-handler";
import { ResetPasswordRequest } from "authentication";
import { map } from "rxjs/operators";
import { ApiErrorTranslatorService } from "../../shared/api-error-translator.service";
import { ValidatorService } from "../../shared/validator.service";

@Component({
  selector: "cinv-reset-password",
  templateUrl: "./reset-password.component.html",
  styleUrls: ["./reset-password.component.scss"],
})
export class ResetPasswordComponent implements OnInit {
  isSubmitted: boolean = false;
  userMessage: string;
  private clientSettings: any;
  resetPasswordRequest: ResetPasswordRequest;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private httpClient: HttpClient,
    private validator: ValidatorService,
    private route: ActivatedRoute,
    private apiErrorTransaltorService: ApiErrorTranslatorService,
    private userService: UserService
  ) {}

  passwordResetForm = this.fb.group(
    {
      userName: [
        "",
        [Validators.required, Validators.pattern(this.validator.emailRegex)],
      ],
      tempPassword: ["", Validators.required],
      newPassword: [
        "",
        [Validators.required, Validators.pattern(this.validator.passwordRegex)],
      ],
      confirmNewPassword: [
        "",
        [Validators.required, Validators.pattern(this.validator.passwordRegex)],
      ],
    },
    { validators: this.validator.comparePasswords }
  );

  get f() {
    return this.passwordResetForm.controls;
  }

  ngOnInit(): void {
    this.httpClient.get("assets/clientSettings.json").subscribe((data) => {
      this.clientSettings = data;
    });

    this.route.queryParams.subscribe((params) => {
      if (params) {
        if (params.emailAddress) {
          this.passwordResetForm.patchValue({ userName: params.emailAddress });
        }

        if (params.data) {
          const paramData = atob(params.data);
          let dataobject = JSON.parse(paramData);

          this.resetPasswordRequest = {
            organizationId: dataobject.orgId,
            userId: dataobject.userId,
            userName: this.passwordResetForm.controls["userName"].value,
            verificationToken: dataobject.verificationToken,
            temporaryPassword: "",
            newPassword: "",
            authenticationType: "",
          };
        }
      }
    });
  }

  onSubmit() {
    this.isSubmitted = true;

    if (this.passwordResetForm.invalid) {
      return;
    }

    this.resetPasswordRequest.newPassword = this.passwordResetForm.controls[
      "newPassword"
    ].value;
    this.resetPasswordRequest.temporaryPassword = this.passwordResetForm.controls[
      "tempPassword"
    ].value;

    this.resetPasswordRequest.authenticationType = this.clientSettings.authenticationType;

    this.userService
      .resetUserPassword(this.resetPasswordRequest)
      .pipe(
        map((response) => {
          if (response) {
            if (isObservableError(response)) {
              this.userMessage = this.apiErrorTransaltorService.translateErrorMessage(
                response.error.error.errorCode
              );
            } else {
              this.passwordResetForm.reset();
              this.isSubmitted = false;
              this.router.navigate(["/login"]);
            }
          }
        })
      )
      .subscribe();
  }
}
