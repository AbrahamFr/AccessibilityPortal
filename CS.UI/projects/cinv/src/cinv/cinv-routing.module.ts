import { NgModule } from "@angular/core";
import { Routes, RouterModule, ExtraOptions } from "@angular/router"; // CLI imports router
import { LandingPageComponent } from "./landing-page/landing-page.component";
import { LoginPageComponent } from "./login-page/login-page.component";
import { MyHomeComponent } from "./my-home/my-home.component";
import { AuthGuard } from "authentication";
import { AccessibilityComponent } from "./accessibility/accessibility.component";
import { LogoutComponent } from "./logout/logout.component";
import { AuditProcessingPageComponent } from "./audit-processing-page/audit-processing-page.component";
import { SendPasswordResetComponent } from "./password-reset/send-password-reset.component";
import { HelpPageComponent } from "./help-page/help-page.component";
import { UserProfilePageComponent } from "./user-profile-page/user-profile-page.component";
import { ErrorComponent } from "./error/error.component";

const routes: Routes = [
  { path: "", redirectTo: "/landing", pathMatch: "full" },
  { path: "landing", component: LandingPageComponent },
  { path: "processing", component: AuditProcessingPageComponent },
  { path: "login", component: LoginPageComponent },
  {
    path: "auditresults",
    canActivate: [AuthGuard],
    loadChildren: () =>
      import("issue-tracker").then((mod) => mod.IssueTrackerModule),
  },
  { path: "myhome", canActivate: [AuthGuard], component: MyHomeComponent },
  { path: "accessibility", component: AccessibilityComponent },
  { path: "help", component: HelpPageComponent },
  { path: "logout", component: LogoutComponent },
  { path: "sendPasswordReset", component: SendPasswordResetComponent },
  { path: "userProfile", component: UserProfilePageComponent },
  { path: "error", component: ErrorComponent },
  { path: "**", redirectTo: "/error" },
]; // sets up routes constant where you define your routes

const routerOptions: ExtraOptions = {
  useHash: false,
  anchorScrolling: "enabled",
  // enableTracing: true, // <-- debugging purposes only
  // ...any other options you'd like to use
};

@NgModule({
  imports: [RouterModule.forRoot(routes, routerOptions)],
  exports: [RouterModule],
})
export class CinvRoutingModule {}
