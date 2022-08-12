import {
  HttpClient,
  HttpErrorResponse,
  HttpResponse,
} from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, Validators } from "@angular/forms";
import { isObservableError } from "projects/api-handler/src/public-api";
import { EMPTY } from "rxjs";
import { map } from "rxjs/operators";
import { ApiErrorTranslatorService } from "../../shared/api-error-translator.service";
import { ValidatorService } from "../../shared/validator.service";
import { PasswordResetService } from "../password-reset-service/password-reset.service";
import { EmailResetRequest } from "./emailResetRequest";

@Component({
  selector: "cinv-email-submission",
  templateUrl: "./email-submission.component.html",
  styleUrls: ["./email-submission.component.scss"],
})
export class EmailSubmissionComponent implements OnInit {
  isSubmitted: boolean;
  userMessage: string;
  private clientSettings: any;

  constructor(
    private fb: FormBuilder,
    private httpClient: HttpClient,
    private validator: ValidatorService,
    private translatorService: ApiErrorTranslatorService,
    private passwordResetService: PasswordResetService
  ) {}

  get f() {
    return this.emailPasswordResetForm.controls;
  }

  emailPasswordResetForm = this.fb.group({
    emailAddress: [
      "",
      Validators.pattern(new RegExp(this.validator.emailRegex)),
    ],
  });

  ngOnInit(): void {
    this.httpClient.get("assets/clientSettings.json").subscribe((data) => {
      this.clientSettings = data;
    });
  }

  onSubmit() {
    this.isSubmitted = true;

    if (this.emailPasswordResetForm.invalid) {
      return;
    }

    const userEmailAddress = this.emailPasswordResetForm.controls[
      "emailAddress"
    ].value;

    const emailResetRequest: EmailResetRequest = {
      userName: userEmailAddress,
      organizationId: this.clientSettings.organizationId,
      authenticationType: this.clientSettings.authenticationType,
    };

    this.passwordResetService
      .sendPasswordResetEmail(emailResetRequest)
      .pipe(
        map((resp) => {
          if (resp) {
            if (isObservableError(resp)) {
              this.userMessage = this.translatorService.translateErrorMessage(
                resp.error.error.errorCode
              );
              return EMPTY;
            }
            this.userMessage = `A Password reset email has been sent to ${userEmailAddress}.
                                Please check your spam or junk folders.  <br />If you do not receive an email, 
                                please contact our support team.`;
            this.emailPasswordResetForm.reset();
          }
        })
      )
      .subscribe();
  }
}
