import { Injectable } from "@angular/core";
import { ActivatedRoute, ParamMap, Params } from "@angular/router";
import { BehaviorSubject, combineLatest, forkJoin } from "rxjs";
import {
  map,
  distinctUntilChanged,
  filter,
  tap,
  take,
  switchMap,
} from "rxjs/operators";
import { UrlParamsService, UrlDataParams } from "navigation";
import {
  AllScanGroupsService,
  CheckpointGroupService,
  Scan,
  ScansService,
} from "api-handler";
import {
  IssueTrackerAppParams,
  IssueTrackerParams,
  IssueTrackerSummary,
  CheckpointGroup,
  OccurrenceParams,
  ScanGroup,
} from "cs-core";
import { InteractionsService } from "interactions";

@Injectable({
  providedIn: "root",
})
export class IssueTrackerParamService {
  readonly issueTrackerUrlDataParams$ = new BehaviorSubject<UrlDataParams>({});

  readonly issueTrackerAppParams$ = new BehaviorSubject<IssueTrackerAppParams>({
    issueTracker: {
      currentPage: 1,
      recordsToReturn: 20,
      checkpointGroupId: "",
    },
    occurrence: {
      currentPage: 1,
      recordsToReturn: 20,
      checkpointGroupId: "",
    },
  });

  readonly issueTrackerParams$ = new BehaviorSubject<IssueTrackerParams>({
    currentPage: 1,
    recordsToReturn: 20,
    checkpointGroupId: "",
  });

  readonly activeQuickFilter$ = new BehaviorSubject<string | undefined>("");
  readonly updatedCheckpointGroupsList = new BehaviorSubject<boolean>(true);
  private numberOfRecordsToFetch: number = 20;

  constructor(
    private urlParamService: UrlParamsService,
    private allScanGroupsService: AllScanGroupsService,
    private scansService: ScansService,
    private route: ActivatedRoute,
    private checkpointGroupService: CheckpointGroupService,
    private interactionsService: InteractionsService
  ) {}

  setIssueTrackerAppParam(issueTrackerAppParams: IssueTrackerAppParams) {
    this.issueTrackerAppParams$.next(issueTrackerAppParams);
  }
  setIssueTrackerUrlDataParams(queryParams: Params) {
    this.issueTrackerUrlDataParams$.next(queryParams);
  }

  checkInitialState() {
    const queryParams = this.route.snapshot.queryParams;
    const previousUrl = this.urlParamService.previousUrl$.value;

    if (Object.keys(queryParams).length === 0) {
      this.setInitialParams();
    } else if (previousUrl.includes("Occurrences")) {
      const issueTrackerAppParams = this.issueTrackerAppParams$.value;
      this.setScanOrScanGroupActive(issueTrackerAppParams.issueTracker);
    } else {
      if (
        queryParams["data"] &&
        (queryParams["scanId"] || queryParams["scanGroupId"])
      ) {
        this.setScanOrScanGroupActive(queryParams);
        const decodedSegmentData = this.urlParamService.decodeHash(
          queryParams.data
        );
        if (decodedSegmentData) {
          //
          // TODO - cleanse bookmarked params before saving
          //
          this.setIssueTrackerAppParam(decodedSegmentData);
          this.setIssueTrackerUrlDataParams(queryParams);
          this.urlParamService.updateUrlParams(queryParams);
        } else {
          this.setInitialParams();
        }
      } else {
        this.setInitialParams();
      }
    }
  }

  setInitialParams() {
    const isNavPopstate = this.urlParamService.getIsNavPopstate();
    const issueTrackerAppParams = this.issueTrackerAppParams$.value;
    const issueTrackerParams = issueTrackerAppParams.issueTracker as IssueTrackerParams;

    if (isNavPopstate) {
      this.setScanOrScanGroupActive(issueTrackerParams);
      this.urlParamService.updateUrlParams(
        this.issueTrackerUrlDataParams$.value
      );
      this.urlParamService.closeIsNavePopstate();
    } else {
      this.allScanGroupsService.scanGroupActive$.next(
        !this.interactionsService.useCInvStyles
      );
      this.issueTrackerParams$.next({
        currentPage: 1,
        recordsToReturn: 20,
        checkpointGroupId: "",
      });
      this.setIssueTrackerAppParam({
        issueTracker: {
          currentPage: 1,
          recordsToReturn: 20,
          checkpointGroupId: "",
        },
        occurrence: {
          currentPage: 1,
          recordsToReturn: 20,
          checkpointGroupId: "",
        },
      });
    }
  }

  setScanOrScanGroupActive(params: IssueTrackerParams) {
    this.allScanGroupsService.scanGroupActive$.next(
      params["scanId"] || this.interactionsService.useCInvStyles ? false : true
    );
  }

  setActiveQuickFilter() {
    const quickFilter = this.issueTrackerAppParams$.value.issueTracker
      .quickFilter;
    this.activeQuickFilter$.next(quickFilter);
  }
  clearQuickFilter() {
    this.activeQuickFilter$.next("");
  }
  toggleQuickFilter(filter: any) {
    if (filter == this.activeQuickFilter$.value) {
      this.clearQuickFilter();
    } else {
      this.activeQuickFilter$.next(filter);
    }
    this.setQuickFilterSearchParams();
  }
  private setQuickFilterSearchParams() {
    const quickFilter = this.activeQuickFilter$.value;
    let summarySearchFilters;
    let searchParamObj: any = {
      severity: "",
      impactRange: "",
      priorityLevel: "",
      checkpointId: "",
      state: "",
    };
    if (quickFilter == "total-failed-quick-filter") {
      summarySearchFilters = ["State Failed"];
      searchParamObj = { ...searchParamObj, ...{ state: ["Failed"] } };
    } else if (quickFilter == "high-severity-failed-quick-filter") {
      summarySearchFilters = ["Severity High", "State Failed"];
      searchParamObj = {
        ...searchParamObj,
        ...{ severity: ["High"], state: ["Failed"] },
      };
    } else {
      summarySearchFilters = [];
    }
    this.refreshData(searchParamObj, summarySearchFilters);
  }

  private refreshData(searchParams: any, summarySearchFilters: string[]) {
    const issueTrackerAppParams = this.issueTrackerAppParams$.value;
    let summaryFilters = issueTrackerAppParams.issueTracker.summaryFilters;
    summaryFilters = {
      ...summaryFilters,
      ...{ issueTrackerSearchFilters: summarySearchFilters },
    };
    const summaryAndSearchParams = {
      ...searchParams,
      ...{ summaryFilters },
    };
    const params = {
      ...summaryAndSearchParams,
      quickFilter: this.activeQuickFilter$.value,
      recordsToReturn: 20,
    };
    this.updateUrlParmsAndRefresh(params);
  }

  filterAndSetInitialParams(): IssueTrackerParams {
    const params = this.issueTrackerParams$.value;
    if (
      params &&
      params.hasOwnProperty("scanGroupId") &&
      !this.interactionsService.useCInvStyles
    ) {
      delete params.scanId;
      this.setIssueTrackerUrlDataParams({ scanGroupId: params.scanGroupId });
    }
    if (params && params.hasOwnProperty("scanId")) {
      delete params.scanGroupId;
      this.setIssueTrackerUrlDataParams({ scanId: params.scanId });
    }
    return params;
  }

  filterScanPropertyOnUpdate(params: Scan | ScanGroup): IssueTrackerAppParams {
    const issueTrackerAppParams = this.issueTrackerAppParams$.value;
    const issueTrackerParams = issueTrackerAppParams.issueTracker;
    const occurrenceParams = issueTrackerAppParams.occurrence;

    if (params.hasOwnProperty("scanId")) {
      delete issueTrackerParams.scanGroupId;
      delete occurrenceParams.scanGroupId;
    }
    if (params.hasOwnProperty("scanGroupId")) {
      delete issueTrackerParams.scanId;
      delete occurrenceParams.scanId;
    }

    const updatedIssueTrackerParams = Object.assign(issueTrackerParams, params);
    const updatedOccurrenceParams = Object.assign(occurrenceParams, params);
    issueTrackerAppParams.issueTracker = updatedIssueTrackerParams;
    issueTrackerAppParams.occurrence = updatedOccurrenceParams;
    this.setIssueTrackerAppParam(issueTrackerAppParams);
    return issueTrackerAppParams;
  }

  selectedScan$ = combineLatest(
    this.route.queryParamMap.pipe(
      map((params: ParamMap) => params.get("scanId")),
      distinctUntilChanged()
    ),
    this.scansService.scansList
  ).pipe(
    filter((values) => values[1].length >= 0),
    map(
      ([scanId, scans]): Scan =>
        (scans && scans.filter((s) => s.scanId == scanId)[0]) ||
        (scans && scans[0])
    ),
    tap((scan: Scan) => {
      this.issueTrackerParams$.next(
        Object.assign({}, this.issueTrackerParams$.value, {
          scanId: scan && scan.scanId,
        })
      );
      if (
        !this.urlParamService.getIsNavPopstate() &&
        !this.allScanGroupsService.scanGroupActive$.value
      ) {
        this.updateIssueTrackerSummary({
          scanDisplayname: scan.displayName,
        });
      }
    })
  );

  selectedScanGroup$ = combineLatest([
    this.route.queryParamMap.pipe(
      map((params: ParamMap) => params.get("scanGroupId")),
      distinctUntilChanged()
    ),
    this.allScanGroupsService.allScanGroupsList,
  ]).pipe(
    filter((values) => values[1].length >= 0),
    tap((values) => {
      if (values[1].length > 0 && !this.interactionsService.useCInvStyles) {
        this.allScanGroupsService.hasScanGroups$.next(true);
      } else {
        this.allScanGroupsService.hasScanGroups$.next(false);
      }
    }),
    map(
      ([scanGroupId, groups]): ScanGroup =>
        (groups && groups.filter((g) => g.scanGroupId == scanGroupId)[0]) ||
        (groups && groups[0])
    ),
    tap((scanGroup: ScanGroup) => {
      this.issueTrackerParams$.next(
        Object.assign({}, this.issueTrackerParams$.value, {
          scanGroupId: scanGroup && scanGroup.scanGroupId,
        })
      );
      if (
        !this.urlParamService.getIsNavPopstate() &&
        this.allScanGroupsService.scanGroupActive$.value
      ) {
        this.updateIssueTrackerSummary({
          scanDisplayname: scanGroup.displayName,
        });
      }
    })
  );

  selectedCheckpointGroup$ = combineLatest([
    this.route.queryParamMap.pipe(
      map((params: ParamMap) => params),
      distinctUntilChanged()
    ),
    forkJoin([
      this.selectedScan$.pipe(take(1)),
      this.selectedScanGroup$.pipe(take(1)),
    ]),
  ]).pipe(
    filter((data) => {
      let hasData = data[1].some((p) => p);
      if (hasData) {
        return true;
      }
      return false;
    }),
    switchMap(() => {
      const currentActiveParams = this.getIssueTrackerParams();
      if (
        currentActiveParams.checkpointGroupId !== "" &&
        !this.updatedCheckpointGroupsList.value
      ) {
        return this.checkpointGroupService.checkpointGroups$;
      }
      const byId = this.allScanGroupsService.scanGroupActive$.value
        ? { scanGroupId: currentActiveParams.scanGroupId }
        : { scanId: currentActiveParams.scanId };
      return this.checkpointGroupService.getCheckpointGroupListBy(byId);
    }),
    filter((groups) => groups.length >= 0),
    map(
      (groups): CheckpointGroup => {
        const issueTrackerAppParams = this.issueTrackerAppParams$.value;
        const issueTrackerParams = issueTrackerAppParams.issueTracker;
        if (
          issueTrackerParams.checkpointGroupId == "" ||
          issueTrackerParams.checkpointGroupId == undefined
        ) {
          return { checkpointGroupId: "", shortDescription: "All" };
        }
        return (
          groups &&
          groups.filter(
            (g) => g.checkpointGroupId == issueTrackerParams.checkpointGroupId
          )[0]
        );
      }
    ),
    tap((checkpointGroup: CheckpointGroup) => {
      if (checkpointGroup) {
        this.issueTrackerParams$.next(
          Object.assign({}, this.issueTrackerParams$.value, {
            checkpointGroupId:
              checkpointGroup && checkpointGroup.checkpointGroupId,
          })
        );
        if (!this.urlParamService.getIsNavPopstate()) {
          this.updateIssueTrackerSummary({
            checkpointGroupDisplayname: checkpointGroup.shortDescription,
          });
        }
        this.updatedCheckpointGroupsList.next(false);
      }
    })
  );

  getIssueTrackerParams(): IssueTrackerParams {
    const issueTrackerAppParams = this.issueTrackerAppParams$.value;
    const issueTrackerParams = issueTrackerAppParams.issueTracker;
    const {
      summaryFilters,
      quickFilter,
      ...filteredIssueTrackerParams
    } = issueTrackerParams;
    const queryParams = this.route.snapshot.queryParams;

    if (Object.keys(queryParams).length == 0) {
      const initialParams = this.filterAndSetInitialParams();
      this.issueTrackerParams$.next(initialParams);
      const appParamsObj = this.updateIssueTrackerAppParamsObj(initialParams);
      this.setIssueTrackerAppParam(appParamsObj);
      const {
        summaryFilters,
        quickFilter,
        ...filteredInitialParams
      } = initialParams;
      return filteredInitialParams;
    }
    return filteredIssueTrackerParams;
  }

  private updateIssueTrackerSummary(param: IssueTrackerSummary) {
    const issueTrackerAppParams = this.issueTrackerAppParams$.value;
    const summaryFilters = issueTrackerAppParams.issueTracker.summaryFilters;
    const params = {
      ...summaryFilters,
      ...param,
    };
    const appParamsObj = this.updateIssueTrackerAppParamsObj({
      summaryFilters: params,
    });
    this.updateIssueTrackerUrlDataParams(appParamsObj);
  }

  private updateIssueTrackerAppParamsObj(
    params: IssueTrackerParams
  ): IssueTrackerAppParams {
    const issueTrackerAppParams = this.issueTrackerAppParams$.value;
    const issueTrackerParams = issueTrackerAppParams.issueTracker as IssueTrackerParams;
    const updatedIssueTrackerParams = Object.assign(issueTrackerParams, params);
    issueTrackerAppParams.issueTracker = updatedIssueTrackerParams;
    this.setIssueTrackerAppParam(issueTrackerAppParams);
    return issueTrackerAppParams;
  }

  updateUrlParmsAndRefresh(params: IssueTrackerParams) {
    const appParamsObj = this.updateIssueTrackerAppParamsObj(params);
    const updatedUrlDataParams = this.updateIssueTrackerUrlDataParams(
      appParamsObj
    );
    this.urlParamService.updateUrlParams(updatedUrlDataParams);
  }

  createIssueTrackerUrlDataParams(
    scanParam: Scan | ScanGroup,
    appParamsObj: IssueTrackerAppParams
  ): UrlDataParams {
    const hash = this.urlParamService.encodeAppData(appParamsObj);
    const updatedUrlDataParamObj = { ...scanParam, ...{ data: hash } };
    this.setIssueTrackerUrlDataParams(updatedUrlDataParamObj);
    return updatedUrlDataParamObj;
  }

  updateIssueTrackerUrlDataParams(
    appParamsObj: IssueTrackerAppParams
  ): UrlDataParams {
    const hash = this.urlParamService.encodeAppData(appParamsObj);
    const updatedUrlDataParamObj = {
      ...this.issueTrackerUrlDataParams$.value,
      ...{ data: hash },
    };
    this.setIssueTrackerUrlDataParams(updatedUrlDataParamObj);
    return updatedUrlDataParamObj;
  }

  handleScanSelected(updatedParam: Scan | ScanGroup) {
    const params = {
      ...updatedParam,
      ...{
        checkpointGroupId: "",
        recordsToReturn: this.numberOfRecordsToFetch,
      },
    };
    const appParamsObj = this.filterScanPropertyOnUpdate(params);
    const updatedUrlDataParams = this.createIssueTrackerUrlDataParams(
      updatedParam,
      appParamsObj
    );
    this.urlParamService.updateUrlParams(updatedUrlDataParams);
    this.updatedCheckpointGroupsList.next(true);
  }
  handleCheckpointGroupSelected(updatedParam: IssueTrackerParams) {
    const params = {
      ...updatedParam,
      ...{
        recordsToReturn: this.numberOfRecordsToFetch,
      },
    };
    const appParamsObj = this.updateIssueTrackerAppParamsObj(params);
    const updatedUrlDataParams = this.updateIssueTrackerUrlDataParams(
      appParamsObj
    );
    this.urlParamService.updateUrlParams(updatedUrlDataParams);
    this.updatedCheckpointGroupsList.next(false);
  }

  getIssueTrackerUrlDataParams() {
    return this.issueTrackerUrlDataParams$.value;
  }
  getIssueTracker() {
    return this.issueTrackerAppParams$.value.issueTracker;
  }
  getIssueTrackerSummaryFilters() {
    return this.issueTrackerAppParams$.value.issueTracker.summaryFilters;
  }
  getIssueTrackerRecordsToReturn() {
    return this.issueTrackerAppParams$.value.issueTracker.recordsToReturn;
  }
  getOccurrences() {
    return this.issueTrackerAppParams$.value.occurrence;
  }
  getOccurrencesSummaryFilters() {
    return this.issueTrackerAppParams$.value.occurrence.summaryFilters;
  }

  navigateToOccurrences(
    params: Partial<OccurrenceParams>,
    occurrenceSummaryParams: Partial<IssueTrackerSummary>
  ) {
    const issueTrackerAppParams = this.issueTrackerAppParams$.value;
    const issueTrackerParams = this.issueTrackerAppParams$.value.issueTracker;
    const occurrenceParams = this.issueTrackerAppParams$.value.occurrence;
    const summaryDetails = {
      ...issueTrackerParams.summaryFilters,
      ...occurrenceSummaryParams,
    };
    occurrenceParams.summaryFilters = occurrenceParams.hasOwnProperty(
      "summaryFilters"
    )
      ? Object.assign(occurrenceParams.summaryFilters, summaryDetails)
      : summaryDetails;
    const sharedParams = this.getSharedParams(issueTrackerParams);
    const combinedSharedAndNewParams = Object.assign(sharedParams, params);
    issueTrackerAppParams.occurrence = Object.assign(
      occurrenceParams,
      combinedSharedAndNewParams
    );
    this.setIssueTrackerAppParam(issueTrackerAppParams);
    return this.updateIssueTrackerUrlDataParams(issueTrackerAppParams);
  }

  getSharedParams(params: IssueTrackerParams) {
    let sharedParamObj;
    for (let param of Object.keys(params)) {
      if (param == "scanId") {
        sharedParamObj = { ...sharedParamObj, ...{ scanId: params["scanId"] } };
      }
      if (param == "scanGroupId") {
        sharedParamObj = {
          ...sharedParamObj,
          ...{ scanGroupId: params["scanGroupId"] },
        };
      }
      if (param == "checkpointGroupId") {
        sharedParamObj = {
          ...sharedParamObj,
          ...{ checkpointGroupId: params["checkpointGroupId"] },
        };
      }
    }
    return sharedParamObj;
  }
}
