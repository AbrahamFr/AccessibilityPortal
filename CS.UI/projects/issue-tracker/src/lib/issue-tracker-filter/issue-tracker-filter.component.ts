import {
  Component,
  Output,
  ViewChildren,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
} from "@angular/core";
import { AbstractControl, FormBuilder, FormGroup } from "@angular/forms";
import { BehaviorSubject } from "rxjs";
import { CheckpointItem } from "cs-core";
import { InteractionsService, keyCodes, supportedKeyCode } from "interactions";
import {
  impactRangeTranslateToString,
  impactRangeTranslateToRange,
} from "navigation";
import { IssueTrackerParamService } from "../issue-tracker-param.service";
import { IssueTrackerService } from "../issue-tracker.service";

@Component({
  selector: "app-issue-tracker-filter",
  templateUrl: "./issue-tracker-filter.component.html",
  styleUrls: ["./issue-tracker-filter.component.scss"],
})
export class IssueTrackerFilterComponent {
  readonly activeFilter = new BehaviorSubject<string>("severity-filter");

  issueTrackerFilterForm: FormGroup;

  @ViewChildren("filterProperties", { read: ElementRef }) filterProperties;
  @Input()
  showFilter: boolean;
  @Input()
  checkpoints: CheckpointItem[];
  @Output()
  closeFilter = new EventEmitter<null>();

  @HostListener("keydown", ["$event"])
  onkeydown(event: KeyboardEvent) {
    if (event.keyCode === keyCodes.ESCAPE) {
      this.onClose(this.issueTrackerFilterForm);
    }
  }

  checkpointControl: AbstractControl | null = null;
  selectedCheckpoint: any = null;

  allSelectedFilters: string[] = [];

  Object = Object;

  formFilters = {
    severity: [
      { name: "High", selected: false },
      { name: "Med", selected: false },
      { name: "Low", selected: false },
    ],
    impactRange: [
      { name: "High", selected: false },
      { name: "Med", selected: false },
      { name: "Low", selected: false },
    ],
    priorityLevel: [
      { name: "1", selected: false },
      { name: "2", selected: false },
      { name: "3", selected: false },
    ],
    checkpointId: [],
    state: [
      { name: "Failed", selected: false },
      { name: "Visual", selected: false },
      { name: "Warning", selected: false },
    ],
  };

  formFilterDisplay = {
    severity: "Severity",
    impactRange: "Impact",
    priorityLevel: "Priority",
    checkpointId: "Checkpoint",
    state: "State",
  };

  constructor(
    private issueTrackerParamService: IssueTrackerParamService,
    private issueTrackerService: IssueTrackerService,
    private fb: FormBuilder,
    private interactionsService: InteractionsService
  ) {
    this.setFormFilters();
    this.issueTrackerFilterForm = this.fb.group(this.buildFormFilters());
    this.setSelectedFiltersDisplay(this.formFilters);
    this.onFormChanges();
  }

  useCInvStyles = this.interactionsService.useCInvStyles;

  setFormFilters() {
    const issueTrackerAppParams = this.issueTrackerParamService
      .issueTrackerAppParams$.value;
    const issueTrackerParams = issueTrackerAppParams.issueTracker;
    for (let urlParam of Object.keys(issueTrackerParams)) {
      for (let filterParam of Object.keys(this.formFilters)) {
        if (urlParam == filterParam) {
          const urlParamCurrentFilters = issueTrackerParams[urlParam];
          if (urlParam == "checkpointId") {
            this.formFilters[urlParam] = urlParamCurrentFilters;
          } else {
            urlParamCurrentFilters &&
              urlParamCurrentFilters.map((filterOption) => {
                let choiceIndex = this.formFilters[filterParam].findIndex(
                  (p) =>
                    p.name ==
                    (urlParam !== "impactRange"
                      ? filterOption
                      : impactRangeTranslateToString(filterOption))
                );
                this.formFilters[filterParam][choiceIndex].selected = true;
              });
          }
        }
      }
    }
  }

  onFormChanges() {
    this.issueTrackerFilterForm.valueChanges.subscribe((val) => {
      this.allSelectedFilters = [];
      for (let filter of Object.keys(val)) {
        if (filter !== "checkpointId") {
          val[filter].map((choice, i) => {
            if (choice) {
              this.allSelectedFilters.push(
                `${this.formFilterDisplay[filter]} ${this.formFilters[filter][i].name}`
              );
            }
          });
        }
        if (filter == "checkpointId") {
          let checkpointValue = this.issueTrackerFilterForm.get("checkpointId");
          if (
            checkpointValue &&
            checkpointValue.value &&
            checkpointValue.value.length &&
            (checkpointValue &&
              checkpointValue.value &&
              checkpointValue.value.length) !== this.selectedCheckpoint
          ) {
            const checkpoint = checkpointValue.value as string;
            this.allSelectedFilters.push(
              `${
                this.formFilterDisplay[filter]
              } ${this.getCheckpointDescription(checkpoint)}`
            );
          }
        }
      }
    });
    this.checkpointControl =
      this.issueTrackerFilterForm &&
      this.issueTrackerFilterForm.get("checkpointId");
    this.selectedCheckpoint =
      this.checkpointControl && this.checkpointControl.value;
  }

  buildControl(filter: string) {
    const arr = this.formFilters[filter].map((option) => {
      return this.fb.control(option.selected);
    });
    return this.fb.array(arr);
  }

  buildFormFilters() {
    let formFiltersObj = new Object();
    for (let formFilter of Object.keys(this.formFilters)) {
      if (formFilter == "checkpointId") {
        formFiltersObj[formFilter] = this.fb.control(
          this.formFilters[formFilter]
        );
      } else {
        formFiltersObj[formFilter] = this.buildControl(formFilter);
      }
    }
    return formFiltersObj;
  }

  getFilterControl(filter: string) {
    return this.issueTrackerFilterForm.get(filter);
  }

  getAllFilterSelections() {
    let filterObj = new Object();
    for (let formFilter of Object.keys(this.formFilters)) {
      filterObj[formFilter] = this.getFilterSelection(formFilter);
    }
    return filterObj;
  }

  getFilterSelection(filter: string) {
    let formValue = this.issueTrackerFilterForm.value;
    if (filter == "checkpointId") {
      return formValue[filter];
    }

    return formValue[filter]
      .map((selected, i) => {
        if (selected) {
          return this.formFilters[filter][i].name;
        }
      })
      .filter((selected) => selected);
  }

  setSelectedFiltersDisplay(formValue: any) {
    for (let filter of Object.keys(formValue)) {
      let filterSelection = this.getFilterSelection(filter);
      if (filterSelection && filterSelection.length) {
        if (filter == "checkpointId") {
          this.allSelectedFilters.push(
            `${this.formFilterDisplay[filter]} ${this.getCheckpointDescription(
              filterSelection
            )}`
          );
        } else {
          filterSelection.map((selection) => {
            this.allSelectedFilters.push(
              `${this.formFilterDisplay[filter]} ${selection}`
            );
          });
        }
      }
    }
  }

  getCheckpointDescription(id: string): string {
    const checkpointList = this.issueTrackerService.checkpointsList$.value;
    const checkpointObj = checkpointList.find((c) => c.checkpointId == id);
    return checkpointObj ? checkpointObj.checkpointDescription : "";
  }

  onClickFilter(evt: MouseEvent) {
    const srcElement = evt.srcElement as HTMLElement;
    this.activeFilter.next(srcElement.id);
    srcElement.focus();
  }

  onOutsideClick(target: HTMLElement) {
    if (this.showFilter && !(target.id == "issue-tracker-filter-button")) {
      this.onClose(this.issueTrackerFilterForm);
    }
  }

  onKeydownFilter(evt: KeyboardEvent | any) {
    const keyCode = evt.which || evt.keyCode;
    const shiftKey = evt.shiftKey;
    const isKeySupported = supportedKeyCode(keyCode);

    if (isKeySupported) {
      const srcElement = evt.srcElement as HTMLElement;
      const filterPropertiesEls = this.filterProperties.toArray();
      const currentIndex = filterPropertiesEls.findIndex((f: ElementRef) =>
        f.nativeElement.contains(srcElement)
      );

      if (keyCode == keyCodes.LEFT_ARROW) {
        const nextIndex =
          currentIndex > 0
            ? currentIndex - 1
            : this.filterProperties.length - 1;
        const nextElement = filterPropertiesEls.find(
          (_, i: number) => i == nextIndex
        );
        filterPropertiesEls[nextIndex].nativeElement.focus();
        this.activeFilter.next(nextElement.nativeElement.id);
      }

      if (keyCode == keyCodes.RIGHT_ARROW) {
        const nextIndex =
          currentIndex == this.filterProperties.length - 1
            ? 0
            : currentIndex + 1;
        const nextElement = filterPropertiesEls.find(
          (_, i: number) => i == nextIndex
        );
        filterPropertiesEls[nextIndex].nativeElement.focus();
        this.activeFilter.next(nextElement.nativeElement.id);
      }
    }
  }

  onCheckpointSelected(checkpoint: CheckpointItem) {
    this.issueTrackerFilterForm.controls.checkpointId.setValue(
      checkpoint.checkpointId
    );
  }

  filterTransform(filter, value) {
    if (value == null || value.length < 1) {
      return "";
    }

    if (filter == "checkpointId") {
      return value;
    }

    if (filter == "impactRange") {
      return value.map((v) => impactRangeTranslateToRange(v));
    }

    return value;
  }

  onClearFilters(event: KeyboardEvent) {
    event.stopPropagation();
    this.issueTrackerFilterForm.reset();
    this.allSelectedFilters = [];
  }

  onSubmit = () => {
    const allFilters = Object.assign(
      {},
      this.issueTrackerFilterForm.value,
      this.getAllFilterSelections()
    );
    let transformedFilters = JSON.parse(
      JSON.stringify(allFilters, this.filterTransform)
    );
    // Reset recordsToReturn back to initial state (20) before API call
    transformedFilters["recordsToReturn"] = 20;
    transformedFilters["quickFilter"] = "";
    let summaryFilters = this.issueTrackerParamService.getIssueTrackerSummaryFilters();
    this.allSelectedFilters
      ? (summaryFilters = {
          ...summaryFilters,
          ...{ issueTrackerSearchFilters: this.allSelectedFilters },
        })
      : null;
    const params = {
      ...transformedFilters,
      ...{
        summaryFilters,
      },
    };
    this.issueTrackerParamService.updateUrlParmsAndRefresh(params);
    this.issueTrackerParamService.clearQuickFilter();
    this.issueTrackerService.resetRefreshIssueTrackerSummaryView();
    this.onClose(this.issueTrackerFilterForm);
  };

  onClose = (form: FormGroup) => {
    this.closeFilter.emit(null);
    if (form) {
      form.reset();
    }
  };
}
