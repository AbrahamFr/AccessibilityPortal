import {
  Component,
  OnInit,
  Output,
  EventEmitter,
  Input
} from "@angular/core";
import { BehaviorSubject, EMPTY, Observable, Subject } from "rxjs";
import { RecentAudit } from "../../audits/recentAudit";
import {
  isObservableError,
  makeObservableError,
  ObservableError,
} from "api-handler";
import { AuditService } from "../../audits/audit-service";
import {
  trigger,
  state,
  style,
  transition,
  animate,
} from "@angular/animations";
import { map } from "rxjs/operators";
import { Router } from "@angular/router";
import { ApiErrorTranslatorService } from "../../shared/api-error-translator.service";
import { UrlParamsService } from "navigation";
import { RecentAuditRequest } from "../../audits/recentAuditRequest";
import { RecentAuditResponse } from "../../audits/recentAuditResponse";
import { CdkRow } from "@angular/cdk/table";
import { Audit } from "../../audits/audit";

@Component({
  selector: "cinv-recent-audits",
  templateUrl: "./recent-audits.component.html",
  styleUrls: ["./recent-audits.component.scss"],
  animations: [
    trigger("detailExpand", [
      state("collapsed", style({ height: "0px", minHeight: "0" })),
      state(
        "expanded",
        style({ height: "*", background: "rgb(239, 239, 239)" })
      ),
      transition(
        "expanded <=> collapsed",
        animate("225ms cubic-bezier(0.4, 0.0, 0.2, 1)")
      ),
    ]),
  ],
})
export class RecentAuditsComponent implements OnInit {
  @Input() selectedAudit: Audit | undefined;
  @Input() refreshData: Subject<boolean> = new Subject<boolean>();
  @Output() changeView = new EventEmitter<{
    pageView: string;
    rowData: object;
  }>();

  recentAuditsResponse$: Observable<RecentAuditResponse | ObservableError>;
  recentAudits$: RecentAudit[];
  totalRecords: number;
  recordsToShow: number = 20;
  recentAuditRequest: RecentAuditRequest;
  currentSelectedRow: Audit;

  constructor(
    private auditService: AuditService,
    private router: Router,
    private urlParamsService: UrlParamsService,
    private apiErrorTranslatorService: ApiErrorTranslatorService
  ) {
    this.recentAuditRequest = {
      currentPage: 1,
      recordsToReturn: this.recordsToShow,
      sortColumn: "",
      sortDirection: "",
    };
  }

  ngOnInit(): void {
    this.populateRecentAudits();

    this.refreshData.subscribe({
      next: (response) => this.populateRecentAudits(),
    });
  }

  getMoreData(pageination: object) {
    this.recordsToShow = pageination["recordsToReturn"];
    this.recentAuditRequest.recordsToReturn = this.recordsToShow;
    this.populateRecentAudits();
  }

  onClick(
    evnt: any,
    expandedElement: MouseEvent | any,
    selectedRow: CdkRow,
    rowIndex: number
  ): string | null {
    expandedElement = expandedElement === selectedRow ? null : selectedRow;
    return expandedElement;
  }

  onDetailBlur(evnt: any, selectedRow: any) {
    if (evnt.relatedTarget.nodeName.toUpperCase() == "TR") {
      return null;
    } else {
      return selectedRow;
    }
  }

  onDetailKeyDown(
    evnt: KeyboardEvent | any,
    expandedElement: any,
    selectedRow: any
  ) {
    const keyCode = evnt.which || evnt.keyCode;

    if (keyCode === 9 && evnt.shiftKey === true) {
      if (expandedElement == undefined) {
        expandedElement = expandedElement === selectedRow ? null : selectedRow;
        return expandedElement;
      } else {
        return selectedRow;
      }
    }

    if (keyCode === 13) {
      this.viewResults(selectedRow);
    }

    return selectedRow;
  }

  onKeyDown(evnt: KeyboardEvent | any, expandedElement: any, selectedRow: any) {
    const keyCode = evnt.which || evnt.keyCode;
 
    if (keyCode === 13 || keyCode === 32) {
      evnt.preventDefault();
      if (expandedElement == undefined) {
        expandedElement = expandedElement === selectedRow ? null : selectedRow;
        return expandedElement;
      }
    }

    if (keyCode === 9 && evnt.shiftKey === true) {
      if (expandedElement == null) {
        return null;
      }
    }

    if (keyCode === 9 && expandedElement != undefined) {
      return selectedRow;
    }

    return null;
  }

  populateRecentAudits() {
    this.auditService
      .getRecentAudits(this.recentAuditRequest)
      .pipe(
        map((response) => {
          if (response) {
            if (isObservableError(response)) {
              return makeObservableError(response.error, response.code);
            }

            this.recentAudits$ = (response as RecentAuditResponse).recentScanList;
            this.totalRecords = (response as RecentAuditResponse).totalRecords;
            if (this.totalRecords < this.recordsToShow) {
              this.recordsToShow = (response as RecentAuditResponse).totalRecords;
            }
          }
        })
      )
      .subscribe();
  }

  displayedColumns$ = new BehaviorSubject<string[]>([
    "auditName",
    "healthScore",
    "healthScoreChangePercent",
    "checkpointGroupDescription",
    "status",
    "startingUrl",
  ]);

  viewEdit($event: any, dataSource: any) {
    this.changeView.emit({ pageView: "Edit", rowData: dataSource });
  }

  viewResults(data: any) {
    const issueTrackerAppParams = {
      issueTracker: {
        currentPage: 1,
        recordsToReturn: 20,
        checkpointGroupId: "",
        summaryFilters: {},
        scanId: data.scanId,
      },

      occurrence: {
        currentPage: 1,
        recordsToReturn: 20,
        checkpointGroupId: "",
        scanId: data.scanId,
      },
    };

    this.router.navigate(["auditresults"], {
      queryParams: {
        scanId: data.scanId,
        data: this.urlParamsService.encodeAppData(issueTrackerAppParams),
      },
    });
  }

  runAudit(scanId: number) {
    this.auditService
      .runAudit(scanId.toString())
      .pipe(
        map((response) => {
          if (response) {
            if (isObservableError(response)) {
              alert(
                `Error Running Audit:\n${this.apiErrorTranslatorService.translateErrorMessage(
                  response.code
                )}`
              );

              return EMPTY;
            }
            this.populateRecentAudits();
          }
        })
      )
      .subscribe();
  }

  deleteAudit(scanId: string) {
    if (confirm("Are you sure you want to delete this audit?")) {
      return this.auditService
        .deleteAudit(scanId)
        .pipe(
          map((response) => {
            if (response) {
              if (isObservableError(response)) {
                alert(
                  `Error Deleting Audit:\n${this.apiErrorTranslatorService.translateErrorMessage(
                    response.code
                  )}`
                );

                return EMPTY;
              }
              this.populateRecentAudits();
            }
          })
        )
        .subscribe();
    }
  }
}
