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
  selector: "app-occurrence-filter-key-attribute",
  templateUrl: "./occurrence-filter-key-attribute.component.html",
  styleUrls: ["../occurrences-filter-picker.component.scss"],
  host: {
    "(document:click)": "onClick($event)",
  },
})
export class OccurrenceFilterKeyAttributeComponent
  extends TypeaheadComponent
  implements OnChanges {
  searchTerms = new BehaviorSubject<string>("");

  @ViewChildren("searchResultsList")
  searchResultsList;
  @ViewChildren("allItemsList")
  allItemsList;
  @Input()
  keyAttribute: FormGroup;

  keyAttributeFilterList: KeyValueString[];

  constructor(
    occurrenceFilterService: OccurrenceFilterService,
    private interactionsService: InteractionsService
  ) {
    super();
    this.keyAttributeFilterList =
      occurrenceFilterService.keyAttributeFilterList;
  }

  inputElementId = "key-attribute-filter-input";
  selectedItemId = "";
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    this.selectedItemId =
      this.keyAttributeFilterList && this.keyAttributeFilterList[0].value;
  }

  searchResult$ = this.searchTerms.pipe(
    debounceTime(100),
    distinctUntilChanged(),
    map((x) => {
      return (
        this.keyAttributeFilterList &&
        this.keyAttributeFilterList.filter((attr) =>
          isSearchMatch(attr.value, x)
        )
      );
    })
  );

  onSearchSelect(keyAttribute?: string) {
    if (keyAttribute !== "key-attribute-filter-input") {
      this.itemSelected.emit(keyAttribute);
      this.toggleDropdown();
      this.searchTerms.next("");
      this.showAllItems.next(false);
      this.showSearchResults.next(false);
    }
  }
}
