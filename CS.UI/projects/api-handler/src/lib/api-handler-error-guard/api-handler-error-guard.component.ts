import { Component, Input, OnInit } from "@angular/core";
import { isObservableError, apiErrorCode } from "../observable-error";
import { isValidationError } from "../error-handler.service";

@Component({
  selector: "api-handler-error-guard",
  templateUrl: "./api-handler-error-guard.component.html",
  styleUrls: ["./api-handler-error-guard.component.scss"],
})
export class ApiHandlerErrorGuardComponent implements OnInit {
  @Input()
  maybeError: any;
  @Input()
  formTemplate?: boolean;
  @Input()
  styles?: any | null = {};

  constructor() {}

  ngOnInit() {}

  get isError() {
    return isObservableError(this.maybeError);
  }

  get errorCode() {
    return apiErrorCode(this.maybeError);
  }

  get isValidationError() {
    return isValidationError(this.errorCode);
  }
}
