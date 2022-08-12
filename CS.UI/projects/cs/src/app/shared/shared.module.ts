import { NgModule } from "@angular/core";
import {
  CommonModule,
  Location,
  LocationStrategy,
  PathLocationStrategy,
} from "@angular/common";
import {
  TranslateModule,
  TranslateLoader,
  TranslateStore,
} from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import {
  HttpClient,
  HttpClientModule,
  HTTP_INTERCEPTORS,
} from "@angular/common/http";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { Title } from "@angular/platform-browser";
import { A11yModule } from "@angular/cdk/a11y";
import { CdkTableModule } from "@angular/cdk/table";
import { CalendarModule, DateAdapter } from "angular-calendar";

import {
  BlobErrorHttpInterceptor,
  APIInterceptor,
  ApiHandlerModule,
  LoadingInterceptor,
  CacheInterceptor,
} from "api-handler";
import { adapterFactory } from "angular-calendar/date-adapters/date-fns";
import { FileUploadComponent } from "../file-upload/file-upload.component";
import { HumanizePipe } from "../../utils/humanize.pipe";
import { ExternalUrlDirective } from "../../utils/external-url.directive";
import { ScanGroupsComponent } from "../scan-groups/scan-groups.component";
import { PageNotFoundComponent } from "../page-not-found/page-not-found.component";
import { CheckpointGroupsComponent } from "../checkpoint-groups/checkpoint-groups.component";
import { InteractionsModule } from "interactions";
import { NavigationModule } from 'navigation';

export function TranslateFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, "./assets/i18n/", "-lang.json");
}

@NgModule({
  declarations: [
    ExternalUrlDirective,
    FileUploadComponent,
    HumanizePipe,
    ScanGroupsComponent,
    CheckpointGroupsComponent,
    PageNotFoundComponent,
  ],
  imports: [
    A11yModule,
    HttpClientModule,
    CommonModule,
    CdkTableModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule.forChild({
      loader: {
        provide: TranslateLoader,
        useFactory: TranslateFactory,
        deps: [HttpClient],
      },
      isolate: true,
    }),
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory,
    }),
    ApiHandlerModule,
    InteractionsModule,
    NavigationModule
  ],
  providers: [
    HttpClient,
    Location,
    { provide: LocationStrategy, useClass: PathLocationStrategy },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: BlobErrorHttpInterceptor,
      multi: true,
    },
    { provide: HTTP_INTERCEPTORS, useClass: APIInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: CacheInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: LoadingInterceptor, multi: true },
    TranslateStore,
    Title,
  ],
  exports: [
    A11yModule,
    HttpClientModule,
    CommonModule,
    CdkTableModule,
    HumanizePipe,
    ExternalUrlDirective,
    FileUploadComponent,
    FormsModule,
    ReactiveFormsModule,
    PageNotFoundComponent,
    ScanGroupsComponent,
    CheckpointGroupsComponent,
    ApiHandlerModule,
    InteractionsModule,
  ],
})
export class SharedModule {}
