import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root",
})
export class ApiErrorTranslatorService {
  constructor() {}

  translateErrorMessage(apiErrorCode: string): string {
    var friendlyErrorMsg: string = "";

    switch (apiErrorCode) {
      case "api:scan:createScan:scanRecordAlreadyExist": {
        friendlyErrorMsg = "Audit Name already exists.";
        break;
      }

      case "api:User:create:userAlreadyExists": {
        friendlyErrorMsg = "An account using that email address already exists";
        break;
      }

      case "api:scan:create:invalidRequest:baseUrlInvalidHttpSchemeNotFound": {
        friendlyErrorMsg = "Base Url address has incorrect format.";
        break;
      }

      case "api:scan:runScan:unableToRunScanDueToCurrentStatus": {
        friendlyErrorMsg = "Scan is currently running";
        break;
      }

      case "api:scan:create:invalidRequest:baseUrlInvalidHttpSchemeNotFound": {
        friendlyErrorMsg = "Invalid Start Url";
        break;
      }

      case "api:scan:run:Forbidden": {
        friendlyErrorMsg = "User is unable to run Audit at this time";
        break;
      }

      case "api:scan:deleteScan:scanInRunningOrPendingState": {
        friendlyErrorMsg = "Unable to delete Audit due to status of the Audit";
        break;
      }

      case "api:scan:runScan:unableToRunScanDueToCurrentStatus": {
        friendlyErrorMsg = "Audit is currently running";
        break;
      }

      case "api:scan:run:Bad Request": {
        friendlyErrorMsg = "Audit is currently running";
        break;
      }

      case "api:Authentication:authenticate:userNotAuthenticated": {
        friendlyErrorMsg = "User does not exist or is not authenticated";
        break;
      }

      case "api:user:resetPassword:updatePasswordFailed": {
        friendlyErrorMsg =
          "Updating your password failed. Please verify username and password.";
        break;
      }

      case "api:scan:runStatus:aborted": {
        friendlyErrorMsg = "There was an error when running the audit";
        break;
      }

      case "api:user:sendPasswordResetLink:userNotFound": {
        friendlyErrorMsg = "User Account was not found associated with this email address.";
        break;
      }

      case "api:user:update:userAlreadyExists": {
        friendlyErrorMsg = "User Account already exists.";
        break;
      }      

      case "api:user:updatePassword:unableToUpdatePwd": {
        friendlyErrorMsg = "Unable to update your password. \n Please verify your current password and try again.";
        break;
      }

      default: {
        friendlyErrorMsg =
          "Unable to process your request at this time, please try again later.";
        break;
      }
    }

    return friendlyErrorMsg;
  }
}
