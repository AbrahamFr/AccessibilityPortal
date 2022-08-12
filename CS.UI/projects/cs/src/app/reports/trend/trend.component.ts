import { Component } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { ActivatedRoute, ParamMap, Router } from "@angular/router";
import { map, distinctUntilChanged } from "rxjs/operators";
import { ChartPerformanceOption } from "../../data-types/types";
import { ScanGroup } from "cs-core";
import { BehaviorSubject, combineLatest } from "rxjs";
import { ScanGroupService } from "../../scan-groups/scan-group.service";
import { AppSettings } from "../../constants/app-settings";
import { UrlParamsService } from "navigation";
import { TrendParamService } from "./trend-param.service";
import { TranslateService } from "@ngx-translate/core";

const pageSettings = require("./scan-history-chart/scan-chart/settings/total-pages-settings.json");

@Component({
  selector: "app-trend",
  templateUrl: "./trend.component.html",
  styleUrls: ["./trend.component.scss"],
})
export class TrendComponent {
  selectedChartOption$ = new BehaviorSubject<ChartPerformanceOption>({
    performanceType: "pagesFail",
    displayName: "Pages - Failed",
    settings: { ...pageSettings },
  });
  constructor(
    private route: ActivatedRoute,
    private scanGroupService: ScanGroupService,
    private titleService: Title,
    private urlParamService: UrlParamsService,
    private trendParamService: TrendParamService,
    private router: Router,
    private readonly translate: TranslateService
  ) {
    this.titleService.setTitle(AppSettings.PAGETITLE + " (Reports - Trend)");
    translate.addLangs(["en"]),
      translate.setDefaultLang("en"),
      translate.use("en");
    this.router.events.subscribe((event) =>
      this.urlParamService.openIsNavPopstate(event)
    );
    this.trendParamService.checkInitialState();
  }

  scanGroups$ = this.scanGroupService.scanGroupList;

  selectedGroup$ = combineLatest(
    this.route.queryParamMap.pipe(
      map((params: ParamMap) => params.get("scanGroupId")),
      distinctUntilChanged()
    ),
    this.scanGroupService.scanGroupList
  ).pipe(
    map(
      ([scanGroupId, groups]): ScanGroup =>
        (groups && groups.filter((g) => g.scanGroupId == scanGroupId)[0]) ||
        (groups && groups[0])
    )
  );

  onScanGroupSelected(scanGroupId) {
    const trendParams = this.trendParamService.trendParams$.value;
    this.trendParamService.trendParams$.next({ scanGroupId });
    this.urlParamService.updateUrlParams(
      Object.assign(trendParams, { scanGroupId })
    );
  }

  onPerformanceOptionSelected(option: ChartPerformanceOption) {
    this.selectedChartOption$.next(option);
  }
}
