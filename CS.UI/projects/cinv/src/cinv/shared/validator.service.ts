import { Injectable } from "@angular/core";
import { AbstractControl, ValidationErrors } from '@angular/forms';

@Injectable({
  providedIn: "root",
})
export class ValidatorService {
  htmlRegex =
    "http(s?)(:\/\/)([a-zA-z0-9-_]+.[a-zA-Z0-9-]+.[a-zA-Z0-9]\/?[a-zA-Z0-9_-]+)[\/]?([a-zA-Z0-9-_])+[\/]?";
  emailRegex = "[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+[.][a-z]+";
  passwordRegex = "(?=.*[a-z])(?=.*[A-Z])+(?=.*[!@#$%^&*_-])+.{8,}";
  nameRegex = "^[a-zA-Z_ ']*$";

  constructor() {}

  comparePasswords(control: AbstractControl): ValidationErrors | null {
    if (
      control &&
      control.get("newPassword") &&
      control.get("confirmNewPassword")
    ) {
      const newPasswordValue = control.get("newPassword")?.value;
      const confirmNewPasswordValue = control.get("confirmNewPassword")?.value;
      return newPasswordValue !== confirmNewPasswordValue
        ? { passwordMatchError: true }
        : null;
    }
    return null;
  }  
}
