import { Component, Input, ViewChildren, OnChanges } from "@angular/core";
import { FormGroup } from "@angular/forms";
import { BehaviorSubject } from "rxjs";
import { debounceTime, distinctUntilChanged, map } from "rxjs/operators";
import { CheckpointItem } from "cs-core";
import {
  TypeaheadComponent,
  isSearchMatch,
  InteractionsService,
} from "interactions";

@Component({
  selector: "app-issue-tracker-filter-checkpoint",
  templateUrl: "./issue-tracker-filter-checkpoint.component.html",
  styleUrls: ["./issue-tracker-filter-checkpoint.component.scss"],
  host: {
    "(document:click)": "onClick($event)",
  },
})
export class IssueTrackerFilterCheckpointComponent
  extends TypeaheadComponent
  implements OnChanges {
  searchTerms = new BehaviorSubject<string>("");

  @ViewChildren("searchResultsList")
  searchResultsList;
  @ViewChildren("allItemsList")
  allItemsList;
  @Input()
  checkpoint: FormGroup;
  @Input()
  checkpoints: CheckpointItem[];

  constructor(private interactionsService: InteractionsService) {
    super();
  }

  inputElementId = "checkpoint-filter-input";
  selectedItemId = "";
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    this.selectedItemId =
      this.checkpoints &&
      this.checkpoints[0] &&
      this.checkpoints[0].checkpointId;
  }

  getCheckpointDescription(id: string): string {
    let checkpointDescription = "";
    if (
      this.checkpoint &&
      this.checkpoint.value &&
      this.checkpoints &&
      this.checkpoints.length > 0
    ) {
      this.checkpoints.map((c) =>
        c.checkpointId == id
          ? (checkpointDescription = c.checkpointDescription)
          : ""
      );
    }
    return checkpointDescription;
  }

  searchResult$ = this.searchTerms.pipe(
    debounceTime(100),
    distinctUntilChanged(),
    map((x) => {
      return (
        this.checkpoints &&
        this.checkpoints.filter((c) =>
          isSearchMatch(c.checkpointDescription, x)
        )
      );
    })
  );

  onSearchSelect(checkpointId?: string) {
    if (checkpointId !== "checkpoint-filter-input") {
      this.itemSelected.emit({ checkpointId });
      this.toggleDropdown();
      this.searchTerms.next("");
      this.showAllItems.next(false);
      this.showSearchResults.next(false);
    }
  }
}
