import { Component, EventEmitter, Input, Output } from "@angular/core";
import { ScanGroup, UrlParams } from "cs-core";

@Component({
  selector: "app-scan-groups",
  templateUrl: "./scan-groups.component.html",
  styleUrls: ["./scan-groups.component.scss"],
})
export class ScanGroupsComponent {
  @Input()
  scanGroups: ScanGroup[];
  @Input()
  selectedScanGroup: ScanGroup;
  @Output()
  scanGroupSelected = new EventEmitter<UrlParams>();

  constructor() {}

  onSelectOptionChange(scanGroupId: number) {
    this.scanGroupSelected.emit({ scanGroupId });
  }
}
