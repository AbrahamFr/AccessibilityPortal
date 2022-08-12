import { NgModule } from "@angular/core";

import { TrendComponent } from "./trend.component";
import { CheckpointFailureComponent } from "./checkpoint-failure/checkpoint-failure.component";
import { CheckpointFailureService } from "./checkpoint-failure/checkpoint-failure.service";
import { CheckpointPerformanceComponent } from "./checkpoint-performance/checkpoint-performance.component";
import { CheckpointPerformanceResultsService } from "./checkpoint-performance/checkpoint-performance-results.service";
import { CheckpointPerformanceGaugeComponent } from "./checkpoint-performance/checkpoint-performance-gauge/checkpoint-performance-gauge.component";
import { PageFailureComponent } from "./page-failure/page-failure.component";
import { PagePerformanceComponent } from "./page-performance/page-performance.component";
import { PagePerformanceResultsService } from "./page-performance/page-performance-results.service";
import { PagePerformanceGaugeComponent } from "./page-performance/page-performance-gauge/page-performance-gauge.component";
import { ScanHistoryChartComponent } from "./scan-history-chart/scan-history-chart.component";
import { ScanChartService } from "./scan-history-chart/scan-chart.service";
import { ScanChartComponent } from "./scan-history-chart/scan-chart/scan-chart.component";
import { ScanPerformanceComponent } from "./scan-performance/scan-performance.component";
import { ScanPerformanceService } from "./scan-performance/scan-performance.service";
import { SearchFilterComponent } from "./search-filter/search-filter.component";
import { TopFailuresComponent } from "./top-failures/top-failures.component";
import { SharedModule } from "../../shared/shared.module";
import { TrendRoutingModule } from "./trend-routing.module";
import { TrendParamService } from './trend-param.service';
import { HttpClient } from "@angular/common/http";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';

export function createTrendTranslateLoader(http: HttpClient) {
  return new TranslateHttpLoader(
    http,
    "./app/reports/trend/i18n/",
    "-lang.json"
  );
}

@NgModule({
  declarations: [
    TrendComponent,
    CheckpointFailureComponent,
    CheckpointPerformanceComponent,
    CheckpointPerformanceGaugeComponent,
    PageFailureComponent,
    PagePerformanceComponent,
    PagePerformanceGaugeComponent,
    ScanHistoryChartComponent,
    ScanChartComponent,
    ScanPerformanceComponent,
    SearchFilterComponent,
    TopFailuresComponent
  ],
  imports: [
    SharedModule,
    TrendRoutingModule,
    TranslateModule.forChild({
      loader: {
        provide: TranslateLoader,
        useFactory: createTrendTranslateLoader,
        deps: [HttpClient]
      },
      isolate: true
    })
  ],
  providers: [
    TrendParamService,
    CheckpointFailureService,
    CheckpointPerformanceResultsService,
    PageFailureComponent,
    PagePerformanceResultsService,
    ScanChartService,
    ScanPerformanceService
  ]
})
export class TrendModule {}
