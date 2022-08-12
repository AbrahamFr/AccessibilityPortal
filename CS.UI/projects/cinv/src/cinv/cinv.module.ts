import { NgModule } from "@angular/core";
import { LocationStrategy, PathLocationStrategy } from "@angular/common";
import {
  HTTP_INTERCEPTORS,
  HttpClient,
  HttpClientModule,
} from "@angular/common/http";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { CdkTableModule } from "@angular/cdk/table";
import {
  TranslateLoader,
  TranslateModule,
  TranslateStore,
} from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";

import { AuthGuard } from "authentication";
import {
  APIInterceptor,
  BlobErrorHttpInterceptor,
  CacheInterceptor,
  LoadingInterceptor,
} from "api-handler";

import { CinvComponent } from "./cinv.component";
import { CinvRoutingModule } from "./cinv-routing.module";
import { HeaderModule } from "./header/header.module";
import { LandingPageComponent } from "./landing-page/landing-page.component";
import { AuditInfoComponent } from "./landing-page/audit-info/audit-info.component";
import { FooterComponent } from "./footer/footer.component";
import { OfferingsComponent } from "./offerings/offerings.component";
import { LoginPageComponent } from "./login-page/login-page.component";
import { LoginFormComponent } from "./login-page/login-form/login-form.component";
import { MyHomeComponent } from "./my-home/my-home.component";
import { AuditFormComponent } from "./my-home/audit-form/audit-form.component";
import { RecentAuditsComponent } from "./my-home/recent-audits/recent-audits.component";
import { AccessibilityComponent } from "./accessibility/accessibility.component";
import { LogoutComponent } from "./logout/logout.component";
import { DateAgoPipe } from "./my-home/recent-audits/pipes/date-ago.pipe";
import { AuditProcessingPageComponent } from "./audit-processing-page/audit-processing-page.component";
import { SendPasswordResetComponent } from "./password-reset/send-password-reset.component";
import { EmailSubmissionComponent } from "./password-reset/email-submission/email-submission.component";
import { ResetPasswordComponent } from "./password-reset/reset-password/reset-password.component";
import { InteractionsModule } from "interactions";
import { HelpPageComponent } from "./help-page/help-page.component";
import { UserProfilePageComponent } from "./user-profile-page/user-profile-page.component";
import { ErrorComponent } from "./error/error.component";

export function TranslateFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, "./assets/i18n/", "-lang.json");
}

@NgModule({
  declarations: [
    CinvComponent,
    LandingPageComponent,
    AuditInfoComponent,
    FooterComponent,
    OfferingsComponent,
    LoginPageComponent,
    LoginFormComponent,
    MyHomeComponent,
    AuditFormComponent,
    RecentAuditsComponent,
    AccessibilityComponent,
    LogoutComponent,
    DateAgoPipe,
    AuditProcessingPageComponent,
    SendPasswordResetComponent,
    EmailSubmissionComponent,
    ResetPasswordComponent,
    HelpPageComponent,
    UserProfilePageComponent,
    ErrorComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    CdkTableModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: TranslateFactory,
        deps: [HttpClient],
      },
      isolate: true,
    }),
    CinvRoutingModule,
    HeaderModule,
    InteractionsModule,
  ],
  providers: [
    AuthGuard,
    HttpClient,
    Location,
    TranslateStore,
    { provide: LocationStrategy, useClass: PathLocationStrategy },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: BlobErrorHttpInterceptor,
      multi: true,
    },
    { provide: HTTP_INTERCEPTORS, useClass: APIInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: CacheInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: LoadingInterceptor, multi: true },
  ],
  bootstrap: [CinvComponent],
})
export class CinvModule {}
