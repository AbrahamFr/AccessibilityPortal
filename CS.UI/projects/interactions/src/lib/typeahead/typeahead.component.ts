import { Component, ElementRef, EventEmitter, Output } from "@angular/core";
import { keyCodes, supportedKeyCode } from "../key-codes";
import { BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";
import { escapeRegExp } from "../search-util";

@Component({
  selector: "interactions-typeahead",
  template: "",
})
export class TypeaheadComponent {
  isDropdownExpanded: boolean = false;
  expandedDropdown = new BehaviorSubject<boolean>(this.isDropdownExpanded);
  showSearchResults = new BehaviorSubject<boolean>(false);
  showAllItems = new BehaviorSubject<boolean>(false);
  searchTerms = new BehaviorSubject<string>("");

  @Output()
  itemSelected = new EventEmitter<any>();

  constructor() {}

  dropdownDisplay = this.expandedDropdown.pipe(
    map((shouldShow) => (shouldShow ? "block" : "none"))
  );

  allItemsDisplay = this.showAllItems.pipe(
    map((shouldShow) => (shouldShow ? "block" : "none"))
  );

  searchResultsDisplay = this.showSearchResults.pipe(
    map((shouldShow) => (shouldShow ? "block" : "none"))
  );

  inputElementId: string | number;
  selectedItemId: string;
  activeListElements: any;
  searchResultsList: any;
  allItemsList: any;

  toggleDropdown() {
    this.isDropdownExpanded = !this.isDropdownExpanded;
    this.expandedDropdown.next(this.isDropdownExpanded);
    if (this.isDropdownExpanded) {
      this.showAllItems.next(true);
      this.scrollToItem();
    }
  }

  onClickInput() {
    this.setInputFocus();
    if (!this.isDropdownExpanded) {
      this.toggleDropdown();
    }
  }

  onClick(evt: MouseEvent) {
    const srcElement = evt.srcElement as HTMLElement;
    const isGlobalClick = srcElement.id == this.inputElementId ? false : true;
    if (this.isDropdownExpanded && isGlobalClick) {
      this.toggleDropdown();
    }
  }

  onKeyDown(evt: KeyboardEvent | any) {
    const keyCode = evt.which || evt.keyCode;
    const isKeySupported = supportedKeyCode(keyCode);
    if (keyCode !== keyCodes.SPACE && isKeySupported) {
      this.activeListElements = this.showSearchResults.value
        ? this.searchResultsList
        : this.allItemsList;
      if (!(this.activeListElements instanceof Array)) {
        this.activeListElements = this.activeListElements.toArray();
      }
      const currentIndex = this.activeListElements.findIndex((item) =>
        item.nativeElement.contains(evt.target)
      );
      const moveUp = currentIndex > 0;
      const moveDown = currentIndex < this.activeListElements.length - 1;

      if (!this.isDropdownExpanded) {
        if (
          !this.isDropdownExpanded &&
          (keyCode == keyCodes.DOWN_ARROW || keyCode == keyCodes.ENTER)
        ) {
          this.toggleDropdown();
        }
      }

      if (this.isDropdownExpanded) {
        evt.preventDefault();
        if (
          this.isDropdownExpanded &&
          keyCode == keyCodes.DOWN_ARROW &&
          moveDown
        ) {
          if (currentIndex >= 0) {
            this.activeListElements[currentIndex + 1].nativeElement.focus();
            setTimeout(() => {
              this.activeListElements[
                currentIndex + 1
              ].nativeElement.scrollIntoView({
                behavior: "auto",
                block: "nearest",
                inline: "start",
              });
            }, 0);
          } else {
            const currentSelectionListIndex = this.activeListElements.findIndex(
              (item) => item.nativeElement.id == this.selectedItemId
            );

            if (currentSelectionListIndex >= 0) {
              this.activeListElements[
                currentSelectionListIndex
              ].nativeElement.focus();
            } else {
              this.activeListElements[0].nativeElement.focus();
              setTimeout(() => {
                this.activeListElements[0].nativeElement.scrollIntoView({
                  behavior: "auto",
                  block: "nearest",
                  inline: "start",
                });
              }, 0);
            }
          }
        }

        if (this.isDropdownExpanded && keyCode == keyCodes.UP_ARROW && moveUp) {
          this.activeListElements[currentIndex - 1].nativeElement.focus();
          setTimeout(() => {
            this.activeListElements[
              currentIndex - 1
            ].nativeElement.scrollIntoView({
              behavior: "auto",
              block: "nearest",
              inline: "start",
            });
          }, 0);
        }

        if (this.isDropdownExpanded && keyCode == keyCodes.ENTER) {
          if (
            evt.srcElement.id == this.inputElementId &&
            evt.srcElement.value == ""
          ) {
            this.onSearchSelect("");
          } else {
            this.onSearchSelect(evt.target.id);
          }
        }

        if (
          this.isDropdownExpanded &&
          (keyCode == keyCodes.TAB || keyCode == keyCodes.ESCAPE)
        ) {
          this.toggleDropdown();
          this.setInputFocus();
        }
      }
    }

    if (
      keyCode == keyCodes.SPACE &&
      this.isDropdownExpanded &&
      evt.srcElement.id !== this.inputElementId
    ) {
      evt.preventDefault();
      this.onSearchSelect(evt.target.id);
    }
  }

  setInputFocus() {
    const inputEl = document.getElementById(
      this.inputElementId as string
    ) as HTMLInputElement;
    if (inputEl) {
      inputEl.select();
      inputEl.focus();
    }
  }

  scrollToItem() {
    const currentSelectedElement: ElementRef<HTMLElement> =
      this.activeListElements &&
      this.activeListElements.find(
        (el) => el.nativeElement.id == this.selectedItemId
      );
    setTimeout(() => {
      if (currentSelectedElement) {
        currentSelectedElement.nativeElement.scrollIntoView({
          behavior: "auto",
          block: "nearest",
          inline: "start",
        });
      }
    }, 0);
  }

  onClickSearch(evt: MouseEvent) {
    evt.stopPropagation();
  }

  onClickTypeahead(evt: MouseEvent) {
    evt.stopImmediatePropagation();
  }

  onSearchChanged(searchValue: string) {
    const searchChar = escapeRegExp(searchValue);
    if (!this.isDropdownExpanded) {
      this.toggleDropdown();
    }
    if (searchChar) {
      this.showAllItems.next(false);
      this.showSearchResults.next(true);
      this.searchTerms.next(searchChar);
    } else {
      this.showSearchResults.next(false);
      this.showAllItems.next(true);
    }
  }

  onSearchSelect(id?: string) {
    if (id !== this.inputElementId) {
      this.itemSelected.emit({ id });
      this.toggleDropdown();
      this.searchTerms.next("");
      this.showAllItems.next(false);
      this.showSearchResults.next(false);
    }
  }
}
