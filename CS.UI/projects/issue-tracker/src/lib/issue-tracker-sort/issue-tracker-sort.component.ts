import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  OnInit,
  Output,
} from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { InteractionsService, keyCodes } from "interactions";
import { IssueTrackerParamService } from "../issue-tracker-param.service";
import { IssueTrackerService } from "../issue-tracker.service";

@Component({
  selector: "app-issue-tracker-sort",
  templateUrl: "./issue-tracker-sort.component.html",
  styleUrls: ["./issue-tracker-sort.component.scss"],
})
export class IssueTrackerSortComponent implements OnInit {
  @Input()
  showFilter: boolean;
  @Output()
  closeSort = new EventEmitter<null>();

  @HostListener("keydown", ["$event"])
  onkeydown(event: KeyboardEvent) {
    if (event.keyCode === keyCodes.ESCAPE) {
      this.onClose(this.issueTrackerSortForm);
    }
  }

  issueTrackerSortForm: FormGroup;
  Object = Object;

  formSortOptions = {
    default: {
      name: "Default",
      selected: false,
      sortDirection: "",
    },
    severityId: {
      name: "Severity",
      selected: false,
      sortDirection: "asc",
    },
    impact: {
      name: "Impact",
      selected: false,
      sortDirection: "desc",
    },
    occurrences: {
      name: "Occurrences",
      selected: false,
      sortDirection: "desc",
    },
    pages: {
      name: "Pages",
      selected: false,
      sortDirection: "desc",
    },
    highestPageLevel: {
      name: "Highest Page Level",
      selected: false,
      sortDirection: "asc",
    },
    priorityLevel: {
      name: "Priority",
      selected: false,
      sortDirection: "asc",
    },
    checkpoint: {
      name: "Checkpoint",
      selected: false,
      sortDirection: "asc",
    },
    state: {
      name: "State",
      selected: false,
      sortDirection: "asc",
    },
  };

  constructor(
    private fb: FormBuilder,
    private issueTrackerParamService: IssueTrackerParamService,
    private issueTrackerService: IssueTrackerService,
    private interactionsService: InteractionsService
  ) {
    this.setSelectedSortOption();
    this.issueTrackerSortForm = this.fb.group({
      sort: [""],
    });
  }
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnInit() {}

  setSelectedSortOption() {
    const issueTrackerParams = this.issueTrackerParamService.getIssueTracker();
    if (issueTrackerParams && issueTrackerParams.sortColumn) {
      for (let sortParam of Object.keys(this.formSortOptions)) {
        if (issueTrackerParams && issueTrackerParams.sortColumn == sortParam) {
          this.formSortOptions[sortParam].selected = true;
        }
      }
    } else {
      this.formSortOptions["default"].selected = true;
    }
  }

  onOutsideClick(target: HTMLElement) {
    if (this.showFilter && !(target.id == "issue-tracker-sort-button")) {
      this.onClose(this.issueTrackerSortForm);
    }
  }

  onSubmit = () => {
    const sortColumn = this.issueTrackerSortForm.value;
    let sortParamObj =
      sortColumn.sort == "default"
        ? { sortColumn: "", sortDirection: "" }
        : {
            sortColumn: sortColumn.sort,
            sortDirection: this.formSortOptions[sortColumn.sort].sortDirection,
          };
    // Reset recordsToReturn back to initial state (20) before API call
    sortParamObj["recordsToReturn"] = 20;
    this.issueTrackerParamService.updateUrlParmsAndRefresh(sortParamObj);
    this.issueTrackerService.resetRefreshIssueTrackerSummaryView();
    this.onClose(this.issueTrackerSortForm);
  };

  onClose = (form: FormGroup) => {
    this.closeSort.emit(null);
    if (form) {
      form.reset();
    }
  };
}
