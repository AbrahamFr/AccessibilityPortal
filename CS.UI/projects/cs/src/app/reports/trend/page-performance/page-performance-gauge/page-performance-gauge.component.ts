import { Component, Input, OnInit, ViewChild, OnChanges } from '@angular/core';
import { ScanGroupPerformanceMetrics } from "../../../../data-types/types";
import {FizzChart}  from "../../../../../assets/js/fizz-charts";

const dataFormat: string = `application/json`

@Component({
  selector: 'app-page-performance-gauge',
  templateUrl: './page-performance-gauge.component.html',
  styleUrls: ["./page-performance-gauge.component.scss"]
})
export class PagePerformanceGaugeComponent implements OnInit, OnChanges {
  @Input()
  data: ScanGroupPerformanceMetrics;
  @Input()
  settings: any;
  @ViewChild("pageGauge", { static: true })

  chart = null;

  constructor() { }

  ngOnInit() {
    if (!this.data) {
      return;
    }
  }

  ngOnChanges(): void {
    if (!this.data) {
      return;
    }
    this.create_chart(this.data, dataFormat)
  }

  private create_chart(data: ScanGroupPerformanceMetrics, format) {
    const containers = {
      chart: document.getElementById('page-perf-gauge') as any
    };
    const failedTotal = data.metrics.failedTotal
    const passedTotal = data.metrics.passedTotal;
    const chartData = [
      {
        status: "passed",
        total: passedTotal
      },
      {
        status: "failed",
        total: failedTotal
      }
    ]
    this.chart = new FizzChart(chartData, format, containers, null, this.settings.options);
  }
}
