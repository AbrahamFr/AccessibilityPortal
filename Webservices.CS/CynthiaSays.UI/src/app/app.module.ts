import { CommonModule } from "@angular/common";
import { BrowserModule } from "@angular/platform-browser";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { Router } from "@angular/router";
import { NgModule } from "@angular/core";
import {
  HttpClient,
  HttpClientModule,
  HTTP_INTERCEPTORS
} from "@angular/common/http";
import { A11yModule } from "@angular/cdk/a11y";

import { AppComponent } from "./app.component";
import { HeaderComponent } from "./header/header.component";
import { AboutComponent } from "./about/about.component";
import { LearnMoreComponent } from "./learn-more/learn-more.component";
import { ReportsComponent } from "./reports/reports.component";
import { TermsOfUseComponent } from "./terms-of-use/terms-of-use.component";
import { FooterComponent } from "./footer/footer.component";
import { TestSiteComponent } from "./test-site/test-site.component";
import { InputValidationErrorComponent } from "./input-validation-error/input-validation-error.component";
import { AppRoutingModule } from "src/routing/app-routing.module";
import {
  TranslateModule,
  TranslateLoader,
  TranslateStore
} from "@ngx-translate/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

export function TranslateFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, "./assets/i18n/", "-lang.json");
}

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    AboutComponent,
    LearnMoreComponent,
    ReportsComponent,
    TermsOfUseComponent,
    FooterComponent,
    TestSiteComponent,
    InputValidationErrorComponent
  ],
  imports: [
    AppRoutingModule,
    CommonModule,
    BrowserModule.withServerTransition({ appId: "serverApp" }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: TranslateFactory,
        deps: [HttpClient]
      },
      isolate: true
    })
  ],
  providers: [TranslateStore, HttpClient],
  exports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    InputValidationErrorComponent
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor(router: Router) {}
}
