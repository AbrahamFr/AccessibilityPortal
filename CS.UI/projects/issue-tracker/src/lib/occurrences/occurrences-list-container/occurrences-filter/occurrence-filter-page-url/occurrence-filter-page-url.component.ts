import { Component, Input, ViewChildren, OnChanges } from "@angular/core";
import {
  TypeaheadComponent,
  isSearchMatch,
  InteractionsService,
} from "interactions";
import { BehaviorSubject } from "rxjs";
import { debounceTime, distinctUntilChanged, map } from "rxjs/operators";
import { FormGroup } from "@angular/forms";
import { OccurrenceFilterService } from "../occurrence-filter.service";

@Component({
  selector: "app-occurrence-filter-page-url",
  templateUrl: "./occurrence-filter-page-url.component.html",
  styleUrls: ["../occurrences-filter-picker.component.scss"],
  host: {
    "(document:click)": "onClick($event)",
  },
})
export class OccurrenceFilterPageUrlComponent
  extends TypeaheadComponent
  implements OnChanges {
  searchTerms = new BehaviorSubject<string>("");

  @ViewChildren("searchResultsList")
  searchResultsList;
  @ViewChildren("allItemsList")
  allItemsList;
  @Input()
  pageUrl: FormGroup;

  urlFilterList: string[];

  constructor(
    occurrenceFilterService: OccurrenceFilterService,
    private interactionsService: InteractionsService
  ) {
    super();
    this.urlFilterList = occurrenceFilterService.urlFilterList;
  }

  inputElementId = "page-url-filter-input";
  selectedItemId = "";
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    this.selectedItemId = this.urlFilterList && this.urlFilterList[0];
  }

  searchResult$ = this.searchTerms.pipe(
    debounceTime(100),
    distinctUntilChanged(),
    map((x) => {
      return (
        this.urlFilterList &&
        this.urlFilterList.filter((url) => isSearchMatch(url, x))
      );
    })
  );

  onSearchSelect(url?: string) {
    if (url !== "page-url-filter-input") {
      this.itemSelected.emit(url);
      this.toggleDropdown();
      this.searchTerms.next("");
      this.showAllItems.next(false);
      this.showSearchResults.next(false);
    }
  }
}
