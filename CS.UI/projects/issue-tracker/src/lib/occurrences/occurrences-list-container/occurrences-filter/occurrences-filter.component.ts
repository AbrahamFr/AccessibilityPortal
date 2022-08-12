import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  Output,
  ViewChildren,
  OnInit,
} from "@angular/core";
import { FormGroup, FormBuilder } from "@angular/forms";
import { BehaviorSubject } from "rxjs";
import { InteractionsService, keyCodes, supportedKeyCode } from "interactions";
import { OccurrenceList } from "../../../types";
import {
  OccurrenceFilterService,
  OccurrenceFilters,
} from "./occurrence-filter.service";

@Component({
  selector: "app-occurrences-filter",
  templateUrl: "./occurrences-filter.component.html",
  styleUrls: ["./occurrences-filter.component.scss"],
})
export class OccurrencesFilterComponent implements OnInit {
  readonly activeFilter = new BehaviorSubject<string>("pageTitle-filter");
  occurrencesFilterForm: FormGroup;
  allSelectedFilters: string[];

  @ViewChildren("filterProperties", { read: ElementRef }) filterProperties;
  @Input()
  showFilter: boolean;
  @Input()
  occurrences: OccurrenceList;
  @Output()
  closeFilter = new EventEmitter<null>();

  @HostListener("keydown", ["$event"])
  onkeydown(event: KeyboardEvent) {
    if (event.keyCode === keyCodes.ESCAPE) {
      this.onClose(this.occurrencesFilterForm);
    }
  }

  Object = Object;

  occurrenceFilters: OccurrenceFilters;

  formFilterDisplay = {
    pageTitle: "Page Title",
    pageUrl: "Page Url",
    element: "Element",
    keyAttribute: "Key Attribute",
    containerId: "Container ID",
  };

  constructor(
    private fb: FormBuilder,
    private occurrenceFilterService: OccurrenceFilterService,
    private interactionsService: InteractionsService
  ) {}

  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnInit(): void {
    this.occurrenceFilters = this.occurrenceFilterService.updateOccurrencesFiltersFromParams(
      this.occurrences
    );
    this.occurrencesFilterForm = this.fb.group(this.buildFormFilters());
    this.setSelectedFiltersDisplay();
  }

  onKeydownFilter(evt: KeyboardEvent | any) {
    const keyCode = evt.which || evt.keyCode;
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

  buildFormFilters() {
    return {
      pageTitle: this.fb.control(this.occurrenceFilters.pageTitle?.value),
      pageUrl: this.fb.control(
        !this.occurrenceFilters.pageUrl
          ? undefined
          : this.occurrenceFilters.pageUrl
      ),
      element: this.fb.control(this.occurrenceFilters.element?.value),
      keyAttribute: this.fb.control(this.occurrenceFilters.keyAttribute?.value),
      containerId: this.fb.control(this.occurrenceFilters.containerId?.value),
    };
  }

  getFilterControl(filter: string) {
    return this.occurrencesFilterForm.get(filter);
  }

  setSelectedFiltersDisplay() {
    this.allSelectedFilters = [];
    if (this.occurrenceFilters.pageTitle?.value) {
      this.allSelectedFilters.push(
        `${this.formFilterDisplay.pageTitle} ${this.occurrenceFilters.pageTitle.value}`
      );
    }
    if (this.occurrenceFilters.pageUrl) {
      this.allSelectedFilters.push(
        `${this.formFilterDisplay.pageUrl} ${this.occurrenceFilters.pageUrl}`
      );
    }
    if (this.occurrenceFilters.element?.value) {
      this.allSelectedFilters.push(
        `${this.formFilterDisplay.element} ${this.occurrenceFilters.element.value}`
      );
    }
    if (this.occurrenceFilters.keyAttribute?.value) {
      this.allSelectedFilters.push(
        `${this.formFilterDisplay.keyAttribute} ${this.occurrenceFilters.keyAttribute.value}`
      );
    }
    if (this.occurrenceFilters.containerId?.value) {
      this.allSelectedFilters.push(
        `${this.formFilterDisplay.containerId} ${this.occurrenceFilters.containerId.value}`
      );
    }
  }

  onPageUrlSelected(pageUrl: string) {
    this.occurrenceFilterService.updateFilterForPageUrl(pageUrl);
    this.occurrencesFilterForm.controls.pageUrl.setValue(pageUrl);
    this.setSelectedFiltersDisplay();
  }

  onPageTitleSelected(pageTitle: string) {
    this.occurrenceFilterService.updateFilterForPageTitle(pageTitle);
    this.occurrencesFilterForm.controls.pageTitle.setValue(pageTitle);
    this.setSelectedFiltersDisplay();
  }

  onKeyAttributeSelected(keyAttribute: string) {
    this.occurrenceFilterService.updateFilterForKeyAttribute(keyAttribute);
    this.occurrencesFilterForm.controls.keyAttribute.setValue(keyAttribute);
    this.setSelectedFiltersDisplay();
  }

  onHtmlElementSelected(htmlElement: string) {
    this.occurrenceFilterService.updateFilterForHtmlElement(htmlElement);
    this.occurrencesFilterForm.controls.element.setValue(htmlElement);
    this.setSelectedFiltersDisplay();
  }

  onContainerIdSelected(containerId: string) {
    this.occurrenceFilterService.updateFilterForContainerId(containerId);
    this.occurrencesFilterForm.controls.containerId.setValue(containerId);
    this.setSelectedFiltersDisplay();
  }

  onClickFilter(evt: MouseEvent) {
    const srcElement = evt.srcElement as HTMLElement;
    this.activeFilter.next(srcElement.id);
    srcElement.focus();
  }

  onClearFilters(event: KeyboardEvent) {
    event.stopPropagation();
    this.occurrencesFilterForm.reset();
    this.occurrenceFilterService.initializeStateOccurrencesFilter();
    this.allSelectedFilters = [];
  }

  onOutsideClick(target: HTMLElement) {
    if (this.showFilter && !(target.id == "occurrences-filter-button")) {
      this.onClose(this.occurrencesFilterForm);
    }
  }

  onClose = (form: FormGroup) => {
    this.closeFilter.emit(null);
    if (form) {
      form.reset();
    }
  };

  onSubmit = () => {
    this.onClose(this.occurrencesFilterForm);
    this.occurrenceFilterService.submitOccurrencesFilter(
      this.allSelectedFilters
    );
  };
}
