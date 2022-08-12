import { Injectable } from '@angular/core';
import { ObservableError  } from "./observable-error";
import { BehaviorSubject } from 'rxjs';

export type ValidationErrorDetails = {
  action: string,
  field: string,
  error: string
}

export const isValidationError = (code: string, serverValidationError?: any): boolean => serverValidationError && serverValidationError[code] ? true : false

export const validationErrorDetails = (code: string, serverValidationError: any): ValidationErrorDetails => serverValidationError[code] 

export const getApiErrorCode = (error: ObservableError): string =>  error.code.replace(/:/g,'.');


@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService {
  readonly serverValidationError$ = new BehaviorSubject<ValidationErrorDetails | null>(null)
  readonly activeError$ = new BehaviorSubject<ObservableError | null>(null)
  
  constructor() { }
  
  handleError(error: ObservableError, serverValidationError?: any) {
    const errorCode = getApiErrorCode(error);
    if (isValidationError(errorCode, serverValidationError)) {
      const errorDetails = validationErrorDetails(errorCode, serverValidationError);
      this.serverValidationError$.next(errorDetails);
    } else {
      this.serverValidationError$.next(null);
    }
    this.activeError$.next(error)
  }
}
