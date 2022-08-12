import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnChanges,
} from "@angular/core";
import { FormGroup, FormControl } from "@angular/forms";
import { ScanGroup } from "cs-core";

import { BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";

@Component({
  selector: "search-filter",
  templateUrl: "./search-filter.component.html",
  styleUrls: ["./search-filter.component.scss"],
})
export class SearchFilterComponent implements OnChanges {
  readonly activeSelectOption = new BehaviorSubject<boolean>(false);
  readonly activeSubmitButton = new BehaviorSubject<boolean>(false);
  @Input()
  scanGroups: ScanGroup[];
  @Input()
  selectedGroup: ScanGroup;
  @Output()
  scanGroupSelected = new EventEmitter<number>();

  searchForm = new FormGroup({
    selectedScanGroup: new FormControl(""),
  });

  exportMenuOptions = ["HTML", "PDF", "CSV"];

  selectOptionChanged = this.activeSelectOption.pipe(
    map((updatedOption) => updatedOption)
  );

  submitButtonActivated = this.activeSubmitButton.pipe(map((button) => button));

  constructor() {}

  ngOnChanges() {
    if (this.selectedGroup) {
      this.searchForm.patchValue({
        selectedScanGroup: this.selectedGroup.displayName,
      });
    }
  }

  onSubmit() {
    const value = this.searchForm.value;
    this.scanGroupSelected.emit(value.selectedScanGroup.scanGroupId);
    this.activeSelectOption.next(false);
    this.activeSubmitButton.next(false);
  }

  onSelectOptionChange() {
    const value = this.searchForm.value;
    if (
      value.selectedScanGroup.scanGroupId &&
      this.selectedGroup.scanGroupId != value.selectedScanGroup.scanGroupId
    ) {
      this.activeSelectOption.next(true);
      this.activeSubmitButton.next(true);
    } else {
      this.activeSelectOption.next(false);
      this.activeSubmitButton.next(false);
    }
  }

  onPrint() {
    window.print();
  }
}
