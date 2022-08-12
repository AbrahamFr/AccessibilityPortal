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
  selector: "app-occurrence-filter-html-element",
  templateUrl: "./occurrence-filter-html-element.component.html",
  styleUrls: ["../occurrences-filter-picker.component.scss"],
  host: {
    "(document:click)": "onClick($event)",
  },
})
export class OccurrenceFilterHtmlElementComponent
  extends TypeaheadComponent
  implements OnChanges {
  searchTerms = new BehaviorSubject<string>("");

  @ViewChildren("searchResultsList")
  searchResultsList;
  @ViewChildren("allItemsList")
  allItemsList;
  @Input()
  htmlElement: FormGroup;

  elementFilterList: KeyValueString[];

  constructor(
    occurrenceFilterService: OccurrenceFilterService,
    private interactionsService: InteractionsService
  ) {
    super();
    this.elementFilterList = occurrenceFilterService.elementFilterList;
  }

  inputElementId = "html-element-filter-input";
  selectedItemId = "";
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnChanges() {
    this.selectedItemId =
      this.elementFilterList && this.elementFilterList[0].value;
  }

  searchResult$ = this.searchTerms.pipe(
    debounceTime(100),
    distinctUntilChanged(),
    map((x) => {
      return (
        this.elementFilterList &&
        this.elementFilterList.filter((element) =>
          isSearchMatch(element.value, x)
        )
      );
    })
  );

  onSearchSelect(htmlElement?: string) {
    if (htmlElement !== "html-element-filter-input") {
      this.itemSelected.emit(htmlElement);
      this.toggleDropdown();
      this.searchTerms.next("");
      this.showAllItems.next(false);
      this.showSearchResults.next(false);
    }
  }
}
