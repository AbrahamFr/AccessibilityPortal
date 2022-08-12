import { Component, Input, OnChanges } from "@angular/core";
import { PagePerformanceResultsService } from "./page-performance-results.service";
import { ScanGroupPerformanceMetrics } from "../../../data-types/types";
import { skipWhile, switchMap, startWith, tap } from "rxjs/operators";
import { Subject } from "rxjs";

const pagePerfSettings = require('./page-performance-gauge/page-perf-settings.json');

@Component({
  selector: "app-page-performance",
  templateUrl: "./page-performance.component.html",
  styleUrls: ["./page-performance.component.scss"]
})
export class PagePerformanceComponent implements OnChanges {
  private changeEvents = new Subject<null>();
  @Input()
  scanGroupId: string;
  @Input()
  hasScanGroups: boolean;

  constructor(
    private pagePerformanceResultsService: PagePerformanceResultsService
  ) {}

  totalPagesScanned: number;
  settings = pagePerfSettings;

  ngOnChanges() {
    this.changeEvents.next(null);
  }

  pagePerformanceResults$ = this.changeEvents.pipe(
    startWith(null),
    skipWhile(_ => Boolean(!this.scanGroupId)),
    switchMap(_ =>
      this.pagePerformanceResultsService
        .pagePerformanceResultsList(this.scanGroupId)
        .pipe(
          tap((result: ScanGroupPerformanceMetrics) =>
            result.hasOwnProperty("scanGroupId")
              ? (this.totalPagesScanned = result.metrics.scanTotal)
              : null
          ),
          startWith(null)
        )
    )
  );
}
