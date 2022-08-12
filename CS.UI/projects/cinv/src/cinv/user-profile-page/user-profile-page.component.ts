import { Component, OnInit } from "@angular/core";
import { FormBuilder, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import {
  APIResponse,
  isObservableError,
  ScanGroupsService,
  UserGroupsService,
  TimeZone,
  TimeZoneService,
  UserService,
} from "api-handler";
import {
  LoginService,
  UpdatePasswordRequest,
  UpdateUserRequest,
  User,
} from "authentication";
import { UpdateScanGroupNameParams, UpdateUserGroupNameParams } from "cs-core";
import { of } from "rxjs";
import { map } from "rxjs/operators";
import { ApiErrorTranslatorService } from "../shared/api-error-translator.service";
import { ValidatorService } from "../shared/validator.service";

@Component({
  selector: "cinv-user-profile-page",
  templateUrl: "./user-profile-page.component.html",
  styleUrls: ["./user-profile-page.component.scss"],
})
export class UserProfilePageComponent implements OnInit {
  isSubmitted: boolean = false;
  isPasswordSubmitted: boolean = false;
  isPasswordError: boolean = false;
  timeZoneList: TimeZone[];
  userProfile: User;
  isUserProfileError: boolean = false;
  userProfileClientMessages: string;
  passwordClientMessages: string;
  userName: string;
  userId: number;

  get f() {
    return this.userProfileForm.controls;
  }

  get pwdform() {
    return this.changePasswordForm.controls;
  }

  userProfileForm = this.fb.group({
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
    email: [
      "",
      [
        Validators.required,
        Validators.pattern(new RegExp(this.validator.emailRegex)),
      ],
    ],
    timeZone: ["", Validators.required],
  });

  changePasswordForm = this.fb.group(
    {
      currentPassword: [
        "",
        [
          Validators.required,
          Validators.pattern(new RegExp(this.validator.passwordRegex)),
        ],
      ],
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

  constructor(
    private fb: FormBuilder,
    private validator: ValidatorService,
    private timeZoneService: TimeZoneService,
    private router: Router,
    private userService: UserService,
    private loginService: LoginService,
    private scanGroupsService: ScanGroupsService,
    private userGroupsService: UserGroupsService,
    private apiTranslatorService: ApiErrorTranslatorService
  ) {}

  ngOnInit(): void {
    if (
      localStorage.getItem("userName") === null ||
      localStorage.getItem("userName") === undefined
    ) {
      this.router.navigate(["/login"]);
    }

    //Initializes User Profile data
    this.setUserProfileData();
  }

  setUserProfileData() {
    //Populate TimeZone List from API
    this.timeZoneService.allTimeZonesList
      .pipe(
        map((response: TimeZone[]) => {
          if (response) {
            this.timeZoneList = response;
          }
        })
      )
      .subscribe();

    //Retrieve UserProfile data
    this.userService
      .getUserByUserName(localStorage.getItem("userName")!)
      .pipe(
        map((response) => {
          if (response) {
            if (isObservableError(response)) {
              this.isUserProfileError = true;
              this.userProfileClientMessages = this.apiTranslatorService.translateErrorMessage(
                response.error.error.errorCode
              );
              return of(response);
            }

            //Set Form Values
            this.userProfile = (response as APIResponse).data["user"];

            this.userProfileForm.patchValue({
              firstName: this.userProfile.firstName,
              lastName: this.userProfile.lastName,
              email: this.userProfile.emailAddress,
              timeZone: this.userProfile.timeZone,
            });
          }
        })
      )
      .subscribe();
  }

  onSubmitPasswordChange() {
    this.isPasswordSubmitted = true;
    this.passwordClientMessages = "";

    if (this.changePasswordForm.invalid) {
      return;
    }

    const updatePasswordRequest: UpdatePasswordRequest = {
      currentPassword: this.changePasswordForm.controls["currentPassword"]
        .value,
      newPassword: this.changePasswordForm.controls["newPassword"].value,
    };

    this.userService
      .updatePassword(updatePasswordRequest)
      .pipe(
        map((response) => {
          if (response) {
            if (isObservableError(response)) {
              this.isPasswordError = true;
              this.passwordClientMessages = this.apiTranslatorService.translateErrorMessage(
                response.error.error.errorCode
              );
              return;
            }

            if ((response as APIResponse).data["success"] == "true") {
              this.passwordClientMessages = "Password successfully updated.";
            }

            //Successful Update
            this.isPasswordError = false;
            this.isPasswordSubmitted = false;
            this.changePasswordForm.reset();
          }
        })
      )
      .subscribe();

    this.isPasswordSubmitted = false;
    this.changePasswordForm.reset();
  }

  onUpdateUserProfile() {
    this.isSubmitted = true;
    this.userProfileClientMessages = "";

    if (this.userProfileForm.invalid) {
      return;
    }

    const updateUserRequest: UpdateUserRequest = {
      userId: this.userProfile.userId,
      userName: this.userProfileForm.controls["email"].value,
      firstName: this.userProfileForm.controls["firstName"].value,
      lastName: this.userProfileForm.controls["lastName"].value,
      emailAddress: this.userProfileForm.controls["email"].value,
      timeZone: this.userProfileForm.controls["timeZone"].value,
    };

    this.userService
      .updateUser(updateUserRequest)
      .pipe(
        map((response) => {
          if (response) {
            if (isObservableError(response)) {
              this.isUserProfileError = true;
              this.userProfileClientMessages = this.apiTranslatorService.translateErrorMessage(
                response.error.error.errorCode
              );

              return;
            }

            if ((response as APIResponse).data["success"] == "true") {
              if (
                this.userProfile.emailAddress !==
                this.userProfileForm.controls["email"].value
              ) {
                const updateUserGroupNameParams: UpdateUserGroupNameParams = {
                  userGroupId: this.userGroupsService.userGroupId,
                  userGroupName: this.userProfileForm.controls["email"].value,
                };
                this.userGroupsService
                  .updateUserGroupName(updateUserGroupNameParams)
                  .subscribe();

                const updateScanGroupNameParams: UpdateScanGroupNameParams = {
                  scanGroupId: this.scanGroupsService.getScanGroupId(),
                  scanGroupDisplayName: this.userProfileForm.controls["email"]
                    .value,
                };
                this.scanGroupsService
                  .updateScanGroupName(updateScanGroupNameParams)
                  .subscribe();

                this.loginService.redirectToLogin();
                this.loginService.logout();
                return;
              }

              this.userProfileClientMessages =
                "User Profile successfully updated.";
            }

            //Successful Update
            this.isSubmitted = false;
            this.setUserProfileData();
          }
        })
      )
      .subscribe();
  }
}
