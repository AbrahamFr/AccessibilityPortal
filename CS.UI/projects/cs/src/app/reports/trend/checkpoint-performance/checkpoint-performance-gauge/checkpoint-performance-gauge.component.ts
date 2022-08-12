import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { ScanGroupPerformanceMetrics } from "../../../../data-types/types";
import {FizzChart}  from "../../../../../assets/js/fizz-charts";

const dataFormat: string = `application/json`

@Component({
  selector: 'app-checkpoint-performance-gauge',
  templateUrl: './checkpoint-performance-gauge.component.html',
  styleUrls: ["./checkpoint-performance-gauge.component.scss"]
})
export class CheckpointPerformanceGaugeComponent implements OnInit {
  @Input()
  data: ScanGroupPerformanceMetrics;
  @Input()
  settings: any;
  @ViewChild("checkpointGauge", { static: true })

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
      chart: document.getElementById('checkpoint-perf-gauge') as any
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
