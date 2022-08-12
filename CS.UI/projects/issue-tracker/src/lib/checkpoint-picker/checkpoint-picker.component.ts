import { Component, Input, OnChanges, ViewChildren } from "@angular/core";
import { BehaviorSubject, combineLatest } from "rxjs";
import { debounceTime, distinctUntilChanged, map } from "rxjs/operators";
import { CheckpointGroupService } from "api-handler";
import { CheckpointGroup } from "cs-core";
import {
  TypeaheadComponent,
  isSearchMatch,
  InteractionsService,
} from "interactions";
import { IssueTrackerService } from "../issue-tracker.service";
import { IssueTrackerParamService } from "../issue-tracker-param.service";

@Component({
  selector: "app-checkpoint-picker",
  templateUrl: "./checkpoint-picker.component.html",
  styleUrls: ["./checkpoint-picker.component.scss"],
  host: {
    "(document:click)": "onClick($event)",
  },
})
export class CheckpointPickerComponent
  extends TypeaheadComponent
  implements OnChanges {
  searchTerms = new BehaviorSubject<string>("");

  @ViewChildren("searchResultsList")
  searchResultsList;
  @ViewChildren("allItemsList")
  allItemsList;
  @Input()
  selectedItem: CheckpointGroup;

  constructor(
    private checkpointGroupService: CheckpointGroupService,
    private issueTrackerParamService: IssueTrackerParamService,
    private issueTrackerService: IssueTrackerService,
    private interactionsService: InteractionsService
  ) {
    super();
    this.getSelectedCheckpointGroup();
  }

  inputElementId = "checkpoint-input";
  selectedItemId = "";
  checkpointGroups$ = this.checkpointGroupService.checkpointGroups$;
  hasScansOrScanGroups$ = this.issueTrackerService.hasScansOrScanGroups$;
  selectedCheckpointGroup = "";
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    this.selectedItemId =
      this.selectedItem && this.selectedItem.checkpointGroupId;
  }

  getSelectedCheckpointGroup() {
    this.checkpointGroupService.checkpointGroups$.subscribe((groups) => {
      const issueTrackerParams = this.issueTrackerParamService.getIssueTracker();
      if (
        !this.selectedItem &&
        issueTrackerParams &&
        issueTrackerParams.checkpointGroupId
      ) {
        groups.map((g) => {
          if (g.checkpointGroupId == issueTrackerParams.checkpointGroupId) {
            this.selectedCheckpointGroup = g.shortDescription;
          }
        });
      }
    });
  }

  searchResult$ = combineLatest(
    this.searchTerms.pipe(debounceTime(100), distinctUntilChanged()),
    this.checkpointGroups$
  ).pipe(
    map(([term, groups]) =>
      term ? groups.filter((g) => isSearchMatch(g.shortDescription, term)) : []
    )
  );

  onSearchSelect(checkpointGroupId?: string) {
    if (checkpointGroupId !== "checkpoint-input") {
      this.itemSelected.emit({ checkpointGroupId });
      this.toggleDropdown();
      this.searchTerms.next("");
      this.showAllItems.next(false);
      this.showSearchResults.next(false);
    }
  }
}
