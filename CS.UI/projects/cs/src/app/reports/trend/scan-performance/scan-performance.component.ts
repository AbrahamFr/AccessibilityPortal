import {
  Component,
  Input,
  OnChanges,
  AfterContentChecked,
} from "@angular/core";
import { BaseUrlResolver, OrganizationIdService } from "navigation";
import { ScanPerformanceService } from "./scan-performance.service";
import { skipWhile, switchMap, startWith } from "rxjs/operators";
import { BehaviorSubject, Subject } from "rxjs";

@Component({
  selector: "app-scan-performance",
  templateUrl: "./scan-performance.component.html",
  styleUrls: ["./scan-performance.component.scss"],
})
export class ScanPerformanceComponent
  implements OnChanges, AfterContentChecked {
  private changeEvents = new Subject<null>();
  constructor(
    private issue: ScanPerformanceService,
    private baseUrlResolver: BaseUrlResolver,
    private organizationIdService: OrganizationIdService
  ) {}

  @Input()
  scanGroupId: string;
  @Input()
  hasScanGroups: boolean;

  displayedColumns$ = new BehaviorSubject<string[]>([
    "scan",
    "pages",
    "checkpoints",
    "successes",
    "failures",
    "percentFailed",
  ]);

  homePath: string;
  orgVirtualDir: string | null;
  baseUrl: string;

  ngOnChanges() {
    this.changeEvents.next(null);
    this.setCDKTableStyles();
  }

  ngAfterContentChecked() {
    this.initializeSummaryLinkUrl();
  }

  scanPerformanceResults$ = this.changeEvents.pipe(
    startWith(null),
    skipWhile((_) => Boolean(!this.scanGroupId)),
    switchMap(() =>
      this.issue
        .scanPerformanceResultsList(this.scanGroupId)
        .pipe(startWith(null))
    )
  );

  initializeSummaryLinkUrl() {
    this.orgVirtualDir = this.organizationIdService.orgVirtualDir;
    this.baseUrl = this.baseUrlResolver.baseUrl;
    this.homePath = `${this.baseUrl}../${this.orgVirtualDir}`;
  }

  // This is necessary to apply sticky headers when scrolling within CDK table.
  // There is an open issue with Angular Dev team to expose directive to allow for easier styling of thead and tbody elements
  setCDKTableStyles() {
    const thead = document.querySelector(
      "table#scan-performance-table > thead"
    );
    const tbody = document.querySelector(
      "table#scan-performance-table > tbody"
    );

    if (thead && tbody) {
      thead.setAttribute(
        "style",
        "display: table; width: 100%; table-layout: fixed"
      );
      tbody.setAttribute(
        "style",
        "display: block; overflow-y: scroll; min-height: 20vh; max-height: 50vh;"
      );
    }
  }
}
