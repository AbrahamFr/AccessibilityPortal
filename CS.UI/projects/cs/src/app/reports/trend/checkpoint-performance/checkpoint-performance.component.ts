import { Component, Input, OnChanges } from "@angular/core";
import { CheckpointPerformanceResultsService } from "./checkpoint-performance-results.service";
import { ScanGroupPerformanceMetrics } from "../../../data-types/types";
import { skipWhile, switchMap, startWith, tap } from "rxjs/operators";
import { Subject } from "rxjs";

const checkpointPerfSettings = require('./checkpoint-performance-gauge/checkpoint-perf-settings.json');

@Component({
  selector: "app-checkpoint-performance",
  templateUrl: "./checkpoint-performance.component.html",
  styleUrls: ["./checkpoint-performance.component.scss"]
})
export class CheckpointPerformanceComponent implements OnChanges {
  private changeEvents = new Subject<null>();
  @Input()
  scanGroupId: string;
  @Input()
  hasScanGroups: boolean;

  constructor(
    private checkpointPerformanceResultsService: CheckpointPerformanceResultsService
  ) {}

  totalCheckpointsTested: number;
  settings = checkpointPerfSettings;

  ngOnChanges() {
    this.changeEvents.next(null);
  }

  checkpointPerformanceResults$ = this.changeEvents.pipe(
    startWith(null),
    skipWhile(_ => Boolean(!this.scanGroupId)),
    switchMap(_ =>
      this.checkpointPerformanceResultsService
        .checkpointPerformanceResultsList(this.scanGroupId)
        .pipe(
          tap((result: ScanGroupPerformanceMetrics) =>
            result.hasOwnProperty("scanGroupId")
              ? (this.totalCheckpointsTested = result.metrics.scanTotal)
              : null
          ),
          startWith(null)
        )
    )
  );
}
