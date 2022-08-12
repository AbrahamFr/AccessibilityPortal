import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output
} from "@angular/core";
import { FormGroup, FormControl } from "@angular/forms";
import { ScanChartService } from "./scan-chart.service";
import { BehaviorSubject, Subject } from "rxjs";
import { map, skipWhile, switchMap, startWith } from "rxjs/operators";
import { ChartPerformanceOption } from "../../../data-types/types";
const checkpointSettings = require('./scan-chart/settings/total-checkpoints-settings.json');
const pageSettings = require('./scan-chart/settings/total-pages-settings.json');

@Component({
  selector: "app-scan-history-chart",
  templateUrl: "./scan-history-chart.component.html",
  styleUrls: ["./scan-history-chart.component.scss"]
})
export class ScanHistoryChartComponent implements OnChanges {
  private changeEvents = new Subject<null>();
  readonly activeSelectOption = new BehaviorSubject<boolean>(false);
  readonly activeSubmitButton = new BehaviorSubject<boolean>(false);
  @Input()
  scanGroupId: string;
  @Input()
  selectedChartOption: ChartPerformanceOption;
  @Input()
  hasScanGroups: boolean;
  @Output()
  performanceOptionSelected = new EventEmitter<ChartPerformanceOption>();

  performanceHistoryForm = new FormGroup({
    selectedPerformanceHistory: new FormControl("")
  });

  selectOptionChanged = this.activeSelectOption.pipe(
    map(updatedOption => updatedOption)
  );

  submitButtonActivated = this.activeSubmitButton.pipe(map(button => button));

  performanceHistoryOptions: ChartPerformanceOption[] = [
    {
      performanceType: "pagesFail",
      displayName: "Pages - Failed",
      settings: {...pageSettings}
    },
    {
      performanceType: "checkpointsFail",
      displayName: "Checkpoints - Failed",
      settings: {...checkpointSettings}
    }
  ];

  constructor(private scanChart: ScanChartService) {}

  ngOnChanges() {
    this.changeEvents.next(null);
    if (this.selectedChartOption) {
      this.performanceHistoryForm.patchValue({
        selectedPerformanceHistory: this.selectedChartOption.displayName
      })
    }
  }

  onSubmit() {
    const value = this.performanceHistoryForm.value;
    this.performanceOptionSelected.emit(value.selectedPerformanceHistory);
    this.activeSelectOption.next(false);
    this.activeSubmitButton.next(false);
  }

  onPerformanceOptionChange() {
    const value = this.performanceHistoryForm.value;
    if (
      value.selectedPerformanceHistory.performanceType &&
      this.selectedChartOption.performanceType !=
        value.selectedPerformanceHistory.performanceType
    ) {
      this.activeSelectOption.next(true);
      this.activeSubmitButton.next(true);
    } else {
      this.activeSelectOption.next(false);
      this.activeSubmitButton.next(false);
    }
  }

  scanChartResults$ = this.changeEvents.pipe(
    startWith(null),
    skipWhile(_ => Boolean(!this.scanGroupId)),
    switchMap(_ =>
      this.scanChart
        .scanChartResultsList(this.scanGroupId)
        .pipe(startWith(null))
    )
  );
}
