import { Component, Input, OnChanges } from "@angular/core";
import { CheckpointFailureService } from "./checkpoint-failure.service";
import { skipWhile, switchMap, startWith } from "rxjs/operators";
import { BehaviorSubject, Subject } from "rxjs";

@Component({
  selector: "app-checkpoint-failure",
  templateUrl: "./checkpoint-failure.component.html",
  styleUrls: ["./checkpoint-failure.component.scss"]
})
export class CheckpointFailureComponent implements OnChanges {
  private changeEvents = new Subject<null>();
  @Input()
  scanGroupId: string;
  @Input()
  hasScanGroups: boolean;

  constructor(private checkPointFailure: CheckpointFailureService) {}

  displayedColumns$ = new BehaviorSubject<string[]>([
    "checkpointId",
    "description",
    "failuresTrend",
    "oneRunBackFailures",
    "currentFailures",
    "priority1Failures",
    "pagesImpacted",
  ]);

  ngOnChanges() {
    this.changeEvents.next(null);
  }

  checkpointFailureResults$ = this.changeEvents.pipe(
    startWith(null),
    skipWhile(_ => Boolean(!this.scanGroupId)),
    switchMap(_ =>
      this.checkPointFailure
        .checkpointFailureResultsList(this.scanGroupId)
        .pipe(startWith(null))
    )
  );
}
