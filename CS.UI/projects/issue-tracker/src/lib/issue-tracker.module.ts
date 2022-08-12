import { NgModule } from "@angular/core";
import { CommonModule, DatePipe } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { CdkTableModule } from "@angular/cdk/table";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";

import { ApiHandlerModule } from "api-handler";
import { CheckpointPickerComponent } from "./checkpoint-picker/checkpoint-picker.component";
import { IssueTrackerExportComponent } from "./issue-tracker-export/issue-tracker-export.component";
import { IssueTrackerFilterCheckpointComponent } from "./issue-tracker-filter/issue-tracker-filter-checkpoint/issue-tracker-filter-checkpoint.component";
import { IssueTrackerFilterComponent } from "./issue-tracker-filter/issue-tracker-filter.component";
import { IssueTrackerListComponent } from "./issue-tracker-list/issue-tracker-list.component";
import { IssueTrackerRoutingModule } from "./issue-tracker-routing.module";
import { IssueTrackerSortComponent } from "./issue-tracker-sort/issue-tracker-sort.component";
import { IssueTrackerSummaryComponent } from "./issue-tracker-summary/issue-tracker-summary.component";
import { IssueTrackerComponent } from "./issue-tracker.component";
import { OccurrencesExportComponent } from "./occurrences/occurrences-export/occurrences-export.component";
import { OccurrencesFilterSummaryComponent } from "./occurrences/occurrences-filter-summary/occurrences-filter-summary.component";
import { OccurrenceSummaryComponent } from "./occurrences/occurrences-list-container/occurrence-summary/occurrence-summary.component";
import { OccurrenceFilterContainerIdComponent } from "./occurrences/occurrences-list-container/occurrences-filter/occurrence-filter-container-id/occurrence-filter-container-id.component";
import { OccurrenceFilterHtmlElementComponent } from "./occurrences/occurrences-list-container/occurrences-filter/occurrence-filter-html-element/occurrence-filter-html-element.component";
import { OccurrenceFilterKeyAttributeComponent } from "./occurrences/occurrences-list-container/occurrences-filter/occurrence-filter-key-attribute/occurrence-filter-key-attribute.component";
import { OccurrenceFilterPageTitleComponent } from "./occurrences/occurrences-list-container/occurrences-filter/occurrence-filter-page-title/occurrence-filter-page-title.component";
import { OccurrenceFilterPageUrlComponent } from "./occurrences/occurrences-list-container/occurrences-filter/occurrence-filter-page-url/occurrence-filter-page-url.component";
import { OccurrencesFilterComponent } from "./occurrences/occurrences-list-container/occurrences-filter/occurrences-filter.component";
import { OccurrencesListContainerComponent } from "./occurrences/occurrences-list-container/occurrences-list-container.component";
import { OccurrencesListComponent } from "./occurrences/occurrences-list-container/occurrences-list/occurrences-list.component";
import { OccurrencesPagesComponent } from "./occurrences/occurrences-list-container/occurrences-pages/occurrences-pages.component";
import { OccurrencesComponent } from "./occurrences/occurrences.component";
import { ScanPickerComponent } from "./scan-picker/scan-picker.component";
import { NavigationModule } from "navigation";
import { InteractionsModule } from "interactions";
import { ExportItemsComponent } from "./export-items/export-items.component";

export function createIssueTrackerTranslateLoader(http: HttpClient) {
  return new TranslateHttpLoader(
    http,
    "./shared-styles/reports/issue-tracker/i18n/",
    "-lang.json"
  );
}

@NgModule({
  declarations: [
    IssueTrackerComponent,
    IssueTrackerListComponent,
    IssueTrackerExportComponent,
    IssueTrackerSummaryComponent,
    IssueTrackerFilterComponent,
    IssueTrackerFilterCheckpointComponent,
    IssueTrackerSortComponent,
    ScanPickerComponent,
    CheckpointPickerComponent,
    OccurrencesComponent,
    OccurrencesFilterSummaryComponent,
    OccurrencesExportComponent,
    OccurrencesListComponent,
    OccurrencesPagesComponent,
    OccurrencesListContainerComponent,
    OccurrenceSummaryComponent,
    OccurrencesFilterComponent,
    OccurrenceFilterPageTitleComponent,
    OccurrenceFilterHtmlElementComponent,
    OccurrenceFilterContainerIdComponent,
    OccurrenceFilterKeyAttributeComponent,
    OccurrenceFilterPageUrlComponent,
    ExportItemsComponent,
  ],
  imports: [
    CommonModule,
    CdkTableModule,
    FormsModule,
    ReactiveFormsModule,
    IssueTrackerRoutingModule,
    TranslateModule.forChild({
      loader: {
        provide: TranslateLoader,
        useFactory: createIssueTrackerTranslateLoader,
        deps: [HttpClient],
      },
      isolate: true,
    }),
    ApiHandlerModule,
    NavigationModule,
    InteractionsModule,
  ],
  providers: [DatePipe],
  exports: [IssueTrackerComponent, ExportItemsComponent],
})
export class IssueTrackerModule {}
