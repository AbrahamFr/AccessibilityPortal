import { Component, Input, OnChanges } from "@angular/core";
import { PageFailureService } from "./page-failure.service";
import { skipWhile, switchMap, startWith } from "rxjs/operators";
import { BehaviorSubject, Subject } from "rxjs";

@Component({
  selector: "app-page-failure",
  templateUrl: "./page-failure.component.html",
  styleUrls: ["./page-failure.component.scss"]
})
export class PageFailureComponent implements OnChanges {
  private changeEvents = new Subject<null>();
  @Input()
  scanGroupId: string;
  @Input()
  hasScanGroups: boolean;

  constructor(private issue: PageFailureService) {}

  displayedColumns$ = new BehaviorSubject<string[]>([
    "pageUrl",
    "failuresTrend",
    "oneRunBackCheckpointFailures",
    "currentCheckpointFailures",
    "priority1Failures",
    "priority2Failures",
    "priority3Failures",
  ]);

  ngOnChanges() {
    this.changeEvents.next(null);
  }

  pageFailureResults$ = this.changeEvents.pipe(
    startWith(null),
    skipWhile(_ => Boolean(!this.scanGroupId)),
    switchMap(_ =>
      this.issue.pageFailureResultsList(this.scanGroupId).pipe(startWith(null))
    )
  );
}
