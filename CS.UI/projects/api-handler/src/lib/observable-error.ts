import { of, Observable } from "rxjs";
import { catchError } from "rxjs/operators";
import { isDevMode } from "@angular/core";
import { HttpErrorResponse } from "@angular/common/http";

export interface ObservableError {
  type: "ComplianceSheriffError";
  code: string;
  error: any;
}

export const isObservableError = (
  something: any
): something is ObservableError =>
  something && (something as ObservableError).type === "ComplianceSheriffError";

export const apiErrorCode = (error: ObservableError): string => {
  if (error.code == "Not Found" || error.code == "Internal Server Error") {
    return "api.notFound.apiError";
  }
  return error.code.replace(/:/g, ".");
};

export const makeObservableError = (
  error: any,
  code: string
): ObservableError => ({ type: "ComplianceSheriffError", code, error });

export const catchObservableError = <T>(code: string) =>
  catchError<T, Observable<ObservableError>>(error => {
    if (error instanceof HttpErrorResponse && error.error.errorCode) {
      return of(makeObservableError(error, error.error.errorCode));
    }
    if (error.statusText && !error.error.errorCode) {
      return of(makeObservableError(error, error.statusText));
    }
    if (isDevMode()) {
      console.error({ error, code });
    }
    return of(makeObservableError(error, code));
  });
