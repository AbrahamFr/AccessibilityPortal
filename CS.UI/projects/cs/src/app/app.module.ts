import { NgModule } from "@angular/core";
import { Router } from "@angular/router";

import { BrowserModule } from "@angular/platform-browser";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { HttpClient } from "@angular/common/http";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { AppComponent } from "./app.component";

import { FooterComponent } from "./footer/footer.component";
import { AuthGuardOrg } from "authentication";
import { RouteErrorResolver } from "../routing/route-error-resolver";
import { AppRoutingModule } from "../routing/app-routing.module";
import { LogoutComponent } from "./header/logout/logout.component";
import { ExampleComponent } from "./example/example.component";
import { RedirectComponent } from "./redirect/redirect.component";
import { HeaderModule } from "./header/header.module";
import { SharedModule } from "./shared/shared.module";
import {
  TranslateModule,
  TranslateLoader,
  TranslateStore,
} from "@ngx-translate/core";

export function TranslateFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, "./assets/i18n/", "-lang.json");
}

@NgModule({
  declarations: [
    AppComponent,
    FooterComponent,
    LogoutComponent,
    ExampleComponent,
    RedirectComponent,
  ],
  imports: [
    AppRoutingModule,
    BrowserModule,
    HeaderModule,
    SharedModule,
    BrowserAnimationsModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: TranslateFactory,
        deps: [HttpClient],
      },
      isolate: true,
    }),
  ],
  providers: [AuthGuardOrg, RouteErrorResolver, TranslateStore],
  bootstrap: [AppComponent],
})
export class AppModule {
  // Diagnostic only: inspect router configuration
  constructor(router: Router) {
    // Use a custom replacer to display function names in the route configs
    // const replacer = (key, value) => (typeof value === 'function') ? value.name : value;
    // console.log('Routes: ', JSON.stringify(router.config, replacer, 2));
  }
}
