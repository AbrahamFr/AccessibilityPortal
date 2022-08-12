import {
  Component,
  ElementRef,
  OnInit,
  Input,
  OnChanges,
  ViewChild,
  ViewEncapsulation
} from "@angular/core";
import {
  ScanHistoryData,
  ChartPerformanceOption
} from "../../../../data-types/types";
import {FizzChart}  from "../../../../../assets/js/fizz-charts";

const dataFormat: string = `application/json`

@Component({
  selector: "app-scan-chart",
  templateUrl: "./scan-chart.component.html",
  styleUrls: ["./scan-chart.component.scss"],
  encapsulation: ViewEncapsulation.None
})
export class ScanChartComponent implements OnInit, OnChanges {
  @ViewChild("barChart", { static: true })
  private chartContainer: ElementRef;

  @Input()
  data: ScanHistoryData[];
  @Input()
  selectedChartOption: ChartPerformanceOption;

  performanceOptionKeys = {
    pagesFail: "failedPages",
    checkpointsFail: "failedCheckpoints"
  };

  chart = null;

  constructor() {}

  ngOnInit(): void {
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

  private create_chart(data, format) {
    const containers = {
      chart: document.getElementById('bar-chart') as any
    };
    const settings = this.selectedChartOption.settings;

    this.chart = new FizzChart(data, format, containers, settings.facets, settings.options);
  }

  onResize() {
  }
}
