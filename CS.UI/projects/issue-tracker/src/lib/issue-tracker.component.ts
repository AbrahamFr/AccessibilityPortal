import { Component } from "@angular/core";
import { Router, ParamMap, ActivatedRoute } from "@angular/router";
import { Title } from "@angular/platform-browser";
import { combineLatest, forkJoin } from "rxjs";
import {
  take,
  map,
  distinctUntilChanged,
  filter,
  switchMap,
  publishReplay,
  refCount,
} from "rxjs/operators";
import { TranslateService } from "@ngx-translate/core";
// import { AppSettings } from "../../constants/app-settings";
import { IssueTrackerParams, ScanGroup } from "cs-core";
import { InteractionsService } from "interactions";
import { UrlParamsService } from "navigation";
import { AllScanGroupsService, ScansService, Scan } from "api-handler";
import { IssueTrackerService } from "./issue-tracker.service";
import { IssueTrackerParamService } from "./issue-tracker-param.service";

@Component({
  selector: "app-issue-tracker",
  templateUrl: "./issue-tracker.component.html",
  styleUrls: ["./issue-tracker.component.scss"],
})
export class IssueTrackerComponent {
  constructor(
    private titleService: Title,
    private issueTrackerService: IssueTrackerService,
    private scansService: ScansService,
    private allScanGroupsService: AllScanGroupsService,
    private urlParamService: UrlParamsService,
    private router: Router,
    private route: ActivatedRoute,
    private issueTrackerParamService: IssueTrackerParamService,
    translate: TranslateService,
    private interactionsService: InteractionsService
  ) {
    // this.titleService.setTitle(
    //   AppSettings.PAGETITLE + " (Reports - Issue Tracker)"  // TODO: Solve this
    // );
    translate.addLangs(["en"]),
      translate.setDefaultLang("en"),
      translate.use("en");
    this.router.events.subscribe((event) =>
      this.urlParamService.openIsNavPopstate(event)
    );
    this.issueTrackerParamService.checkInitialState();
  }

  isExportActive$ = this.issueTrackerService.exportActive$;
  scans$ = this.scansService.scansList;
  scanGroups$ = this.allScanGroupsService.allScanGroupsList;
  scanGroupActive$ = this.allScanGroupsService.scanGroupActive$;
  hasScanGroups$ = this.allScanGroupsService.hasScanGroups$;
  isNavPopstate = this.urlParamService.getIsNavPopstate();
  selectedScan$ = this.issueTrackerParamService.selectedScan$;
  selectedScanGroup$ = this.issueTrackerParamService.selectedScanGroup$;
  selectedCheckpointGroup$ = this.issueTrackerParamService
    .selectedCheckpointGroup$;
  useCInvStyles = this.interactionsService.useCInvStyles;

  issueTrackerData$ = combineLatest(
    forkJoin(
      this.selectedScan$.pipe(take(1)),
      this.selectedScanGroup$.pipe(take(1)),
      this.selectedCheckpointGroup$.pipe(take(1))
    ),
    this.route.queryParamMap.pipe(
      map((params: ParamMap) => params),
      distinctUntilChanged()
    )
  ).pipe(
    filter((data) => this.issueTrackerService.filterOnHasScanOrScanGroup(data)),
    switchMap((_) =>
      this.issueTrackerService.getIssueTrackerList(
        this.issueTrackerParamService.getIssueTrackerParams()
      )
    ),
    publishReplay(1),
    refCount()
  );

  onScanSelected(updatedParam: Scan | ScanGroup) {
    this.issueTrackerParamService.handleScanSelected(updatedParam);
  }

  onCheckpointGroupSelected(updatedParam: IssueTrackerParams) {
    this.issueTrackerParamService.handleCheckpointGroupSelected(updatedParam);
  }
}
