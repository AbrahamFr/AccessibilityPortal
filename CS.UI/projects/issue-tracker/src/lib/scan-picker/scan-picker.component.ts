import { Component, Input, OnChanges, ViewChildren } from "@angular/core";
import { debounceTime, distinctUntilChanged, map } from "rxjs/operators";
import { BehaviorSubject, combineLatest, of } from "rxjs";
import { AllScanGroupsService, Scan, ScansService } from "api-handler";
import { ScanGroup } from "cs-core";
import {
  TypeaheadComponent,
  isSearchMatch,
  InteractionsService,
} from "interactions";
import { IssueTrackerService } from "../issue-tracker.service";

@Component({
  selector: "app-scan-picker",
  templateUrl: "./scan-picker.component.html",
  styleUrls: ["./scan-picker.component.scss"],
  host: {
    "(document:click)": "onClick($event)",
  },
})
export class ScanPickerComponent
  extends TypeaheadComponent
  implements OnChanges {
  searchTerms = new BehaviorSubject<string>("");

  @ViewChildren("searchResultsList")
  searchResultsList;
  @ViewChildren("allItemsList")
  allItemsList;
  @ViewChildren("scanSelectOptions")
  scanSelectOptions;
  @Input()
  scans: Scan[];
  @Input()
  selectedScan: Scan;
  @Input()
  scanGroups: ScanGroup[];
  @Input()
  selectedScanGroup: ScanGroup;

  constructor(
    private scansService: ScansService,
    private allScanGroupsService: AllScanGroupsService,
    private issueTrackerService: IssueTrackerService,
    private interactionsService: InteractionsService
  ) {
    super();
  }

  inputElementId = "scan-input";
  selectedItemId = "";
  hasScansOrScanGroups$ = this.issueTrackerService.hasScansOrScanGroups$;
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    this.allScanGroupsService.checkOnUpdateForScanGroup();

    if (this.scanGroupActive$.value) {
      this.selectedItemId =
        this.selectedScanGroup && this.selectedScanGroup.scanGroupId.toString();
    } else {
      this.selectedItemId =
        this.selectedScan && this.selectedScan.scanId.toString();
    }
  }

  hasScanGroups$ = this.allScanGroupsService.hasScanGroups$;
  scanGroupActive$ = this.allScanGroupsService.scanGroupActive$;

  scanGroupVisibility = this.scanGroupActive$.pipe(
    map((shouldShow) => (shouldShow ? true : false))
  );

  onClickScanSelect(evt: MouseEvent) {
    const scanSelectOptionEls = this.scanSelectOptions.toArray();
    const currentIndex = scanSelectOptionEls.findIndex((b) =>
      b.nativeElement.contains(evt.target)
    );
    if (this.allScanGroupsService.getHasScanGroupActiveValue()) {
      if (
        !this.allScanGroupsService.getScanGroupActive() &&
        currentIndex === 1
      ) {
        scanSelectOptionEls[currentIndex].nativeElement.focus();
        this.updateActiveOption();
      }

      if (
        this.allScanGroupsService.getScanGroupActive() &&
        currentIndex === 0
      ) {
        scanSelectOptionEls[currentIndex].nativeElement.focus();
        this.updateActiveOption();
      }
    }

    this.onSearchSelect(
      Number(
        this.allScanGroupsService.getScanGroupActive()
          ? this.selectedScanGroup && this.selectedScanGroup.scanGroupId
          : this.selectedScan && this.selectedScan.scanId
      )
    );
  }

  updateActiveOption() {
    this.allScanGroupsService.switchScanGroupActive();
  }

  searchScanResult$ = combineLatest(
    this.searchTerms.pipe(debounceTime(100), distinctUntilChanged()),
    this.scansService.scansList
  ).pipe(
    map(([term, scans]) =>
      term ? scans.filter((s) => isSearchMatch(s.displayName, term)) : []
    )
  );

  searchScanGroupResult$ = combineLatest(
    this.searchTerms.pipe(debounceTime(100), distinctUntilChanged()),
    this.allScanGroupsService.allScanGroupsList
  ).pipe(
    map(([term, groups]) =>
      term ? groups.filter((g) => isSearchMatch(g.displayName, term)) : []
    )
  );

  onSearchSelect(id?: string | number) {
    if (id && id !== "scan-input") {
      this.itemSelected.emit(
        this.scanGroupActive$.value ? { scanGroupId: id } : { scanId: id }
      );
      this.toggleDropdown();
      this.searchTerms.next("");
      this.showAllItems.next(false);
      this.showSearchResults.next(false);
    }
  }
}
