import { Component, Input, ViewChildren, OnChanges } from "@angular/core";
import { FormGroup } from "@angular/forms";
import { BehaviorSubject } from "rxjs";
import { debounceTime, distinctUntilChanged, map } from "rxjs/operators";
import {
  TypeaheadComponent,
  isSearchMatch,
  InteractionsService,
} from "interactions";
import { KeyValueString } from "cs-core";
import { OccurrenceFilterService } from "../occurrence-filter.service";

@Component({
  selector: "app-occurrence-filter-page-title",
  templateUrl: "./occurrence-filter-page-title.component.html",
  styleUrls: ["../occurrences-filter-picker.component.scss"],
  host: {
    "(document:click)": "onClick($event)",
  },
})
export class OccurrenceFilterPageTitleComponent
  extends TypeaheadComponent
  implements OnChanges {
  searchTerms = new BehaviorSubject<string>("");

  @ViewChildren("searchResultsList")
  searchResultsList;
  @ViewChildren("allItemsList")
  allItemsList;
  @Input()
  pageTitle: FormGroup;

  titleFilterList: KeyValueString[];

  constructor(
    occurrenceFilterService: OccurrenceFilterService,
    private interactionsService: InteractionsService
  ) {
    super();
    this.titleFilterList = occurrenceFilterService.titleFilterList;
  }

  inputElementId = "page-title-filter-input";
  selectedItemId = "";
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    this.selectedItemId = this.titleFilterList && this.titleFilterList[0].value;
  }

  searchResult$ = this.searchTerms.pipe(
    debounceTime(100),
    distinctUntilChanged(),
    map((x) => {
      return (
        this.titleFilterList &&
        this.titleFilterList.filter((title) => isSearchMatch(title.value, x))
      );
    })
  );

  onSearchSelect(title?: string) {
    if (title !== "page-title-filter-input") {
      this.itemSelected.emit(title);
      this.toggleDropdown();
      this.searchTerms.next("");
      this.showAllItems.next(false);
      this.showSearchResults.next(false);
    }
  }
}
