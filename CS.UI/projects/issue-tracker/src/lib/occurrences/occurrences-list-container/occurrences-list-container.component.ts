import { Component, OnInit, ViewChildren } from "@angular/core";
import { ActivatedRoute, ParamMap } from "@angular/router";
import { BehaviorSubject } from "rxjs";
import {
  map,
  distinctUntilChanged,
  switchMap,
  publishReplay,
  refCount,
} from "rxjs/operators";
import { InteractionsService, keyCodes, supportedKeyCode } from "interactions";
import { OccurrenceSummary } from "../../types";
import { OccurrencesService } from "../occurrences.service";
import { IssueTrackerParamService } from "../../issue-tracker-param.service";
import { OccurrencesParamService } from "../occurrences-params.service";

@Component({
  selector: "app-occurrences-list-container",
  templateUrl: "./occurrences-list-container.component.html",
  styleUrls: ["./occurrences-list-container.component.scss"],
})
export class OccurrencesListContainerComponent implements OnInit {
  @ViewChildren("occurrencesListTabs")
  occurrencesListTabs;

  constructor(
    private occurrencesParamService: OccurrencesParamService,
    private occurrenceService: OccurrencesService,
    private route: ActivatedRoute,
    private issueTrackerParamService: IssueTrackerParamService,
    private interactionsService: InteractionsService
  ) {
    this.setOccurrenceSearchFilters();
  }

  isOccurrencesExportActive$ = this.occurrenceService.occurrencesExportActive$;
  readonly activeOccurrenceFilter$ = new BehaviorSubject<boolean>(false);

  occurrenceSummary: OccurrenceSummary;
  occurrenceSearchFilters: string[] | undefined = [];
  useCInvStyles = this.interactionsService.useCInvStyles;

  occurrencesListVisibility$ = this.occurrenceService.activeOccurrencesListTab$.pipe(
    map((activeTab) => (activeTab == "occurrences" ? true : false))
  );

  ngOnInit() {
    const summaryFilters = this.issueTrackerParamService.getOccurrencesSummaryFilters();
    if (summaryFilters) {
      this.occurrenceSummary = {
        issue: summaryFilters.issue,
        totalOccurrences: summaryFilters.totalOccurrences,
        totalPages: summaryFilters.totalPages,
        priorityLevel: summaryFilters.priorityLevel,
        state: summaryFilters.state,
      };
      this.occurrenceSearchFilters =
        summaryFilters && summaryFilters.occurrenceSearchFilters;
    }
  }

  setOccurrenceSearchFilters() {
    this.route.queryParamMap.subscribe((_) => {
      const summaryFilters = this.issueTrackerParamService.getOccurrencesSummaryFilters();
      this.occurrenceSearchFilters =
        summaryFilters && summaryFilters.occurrenceSearchFilters;
    });
  }

  occurrencesList$ = this.route.queryParamMap
    .pipe(
      map((params: ParamMap) => params),
      distinctUntilChanged()
    )
    .pipe(
      switchMap(() => {
        const occurrenceParams = this.issueTrackerParamService.getOccurrences();
        const {
          summaryFilters,
          ...filteredOccurrenceParams
        } = occurrenceParams;
        return this.occurrenceService.getOccurrencesList(
          filteredOccurrenceParams
        );
      }),
      publishReplay(1),
      refCount()
    );

  occurrencesByPage$ = this.route.queryParamMap
    .pipe(
      map((params: ParamMap) => params),
      distinctUntilChanged()
    )
    .pipe(
      switchMap(() => {
        const {
          summaryFilters,
          ...filteredOccurrenceParams
        } = this.issueTrackerParamService.getOccurrences();
        return this.occurrenceService.getOccurrencesByPage(
          filteredOccurrenceParams
        );
      }),
      publishReplay(1),
      refCount()
    );

  clearRecordsToReturn() {
    this.occurrencesParamService.clearRecordsToReturn();
  }

  onClickOccurrencesListTab(evt: MouseEvent) {
    const activeTab = this.occurrenceService.getActiveOccurrencesListTab();
    const listTabEls = this.occurrencesListTabs.toArray();
    const currentIndex = listTabEls.findIndex((t) =>
      t.nativeElement.contains(evt.target)
    );

    if (activeTab == "occurrences" && currentIndex === 1) {
      this.clearRecordsToReturn();
      listTabEls[currentIndex].nativeElement.focus();
      this.occurrenceService.setActiveOccurrencesListTab("pages");
    }

    if (activeTab == "pages" && currentIndex === 0) {
      this.clearRecordsToReturn();
      listTabEls[currentIndex].nativeElement.focus();
      this.occurrenceService.setActiveOccurrencesListTab("occurrences");
    }
  }

  onKeydownOccurrencesListTab(evt: KeyboardEvent) {
    const keyCode = evt.which || evt.keyCode;
    const shiftKey = evt.shiftKey;
    const isKeySupported = supportedKeyCode(keyCode);

    if (isKeySupported) {
      const activeTab = this.occurrenceService.getActiveOccurrencesListTab();
      const listTabEls = this.occurrencesListTabs.toArray();
      const currentIndex = listTabEls.findIndex((t) =>
        t.nativeElement.contains(evt.target)
      );

      if (keyCode === keyCodes.TAB && shiftKey) {
        listTabEls[currentIndex].nativeElement.blur();
      }

      if (keyCode === keyCodes.LEFT_ARROW || keyCode === keyCodes.RIGHT_ARROW) {
        const nextActiveTabIndex = currentIndex === 0 ? 1 : 0;
        listTabEls[nextActiveTabIndex].nativeElement.focus();
        this.occurrenceService.setActiveOccurrencesListTab(
          activeTab == "occurrences" ? "pages" : "occurrences"
        );
        this.clearRecordsToReturn();
      }
    }
  }

  onFilterClick() {
    this.activeOccurrenceFilter$.next(!this.activeOccurrenceFilter$.value);
  }

  onCloseFilter() {
    this.activeOccurrenceFilter$.next(!this.activeOccurrenceFilter$.value);
    this.setFilterButtonFocus();
  }

  setFilterButtonFocus() {
    const filterBtn = document.getElementById(
      "occurrences-filter-button"
    ) as HTMLInputElement;
    if (filterBtn) {
      setTimeout(() => {
        filterBtn.focus();
      }, 0);
    }
  }
}
