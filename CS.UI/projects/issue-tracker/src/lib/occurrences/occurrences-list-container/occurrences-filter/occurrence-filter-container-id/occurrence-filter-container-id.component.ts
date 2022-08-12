import { Component, Input, ViewChildren, OnChanges } from "@angular/core";
import { FormGroup } from "@angular/forms";
import { BehaviorSubject } from "rxjs";
import { debounceTime, distinctUntilChanged, map } from "rxjs/operators";
import { KeyValueString } from "cs-core";
import {
  TypeaheadComponent,
  isSearchMatch,
  InteractionsService,
} from "interactions";
import { OccurrenceFilterService } from "../occurrence-filter.service";

@Component({
  selector: "app-occurrence-filter-container-id",
  templateUrl: "./occurrence-filter-container-id.component.html",
  styleUrls: ["../occurrences-filter-picker.component.scss"],
  host: {
    "(document:click)": "onClick($event)",
  },
})
export class OccurrenceFilterContainerIdComponent
  extends TypeaheadComponent
  implements OnChanges {
  searchTerms = new BehaviorSubject<string>("");

  @ViewChildren("searchResultsList")
  searchResultsList;
  @ViewChildren("allItemsList")
  allItemsList;
  @Input()
  containerId: FormGroup;

  containerIdFilterList: KeyValueString[];

  constructor(
    occurrenceFilterService: OccurrenceFilterService,
    private interactionsService: InteractionsService
  ) {
    super();
    this.containerIdFilterList = occurrenceFilterService.containerIdFilterList;
  }

  inputElementId = "container-id-filter-input";
  selectedItemId = "";
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    this.selectedItemId =
      this.containerIdFilterList && this.containerIdFilterList[0].value;
  }

  searchResult$ = this.searchTerms.pipe(
    debounceTime(100),
    distinctUntilChanged(),
    map((x) => {
      return (
        this.containerIdFilterList &&
        this.containerIdFilterList.filter((id) => isSearchMatch(id.value, x))
      );
    })
  );

  onSearchSelect(containerId?: string) {
    if (containerId !== "container-id-filter-input") {
      this.itemSelected.emit(containerId);
      this.toggleDropdown();
      this.searchTerms.next("");
      this.showAllItems.next(false);
      this.showSearchResults.next(false);
    }
  }
}
