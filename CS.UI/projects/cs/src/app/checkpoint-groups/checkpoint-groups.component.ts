import { Component, EventEmitter, Input, Output } from "@angular/core";
import { CheckpointGroup } from "cs-core";
import { UrlParams } from "cs-core";

@Component({
  selector: "app-checkpoint-groups",
  templateUrl: "./checkpoint-groups.component.html",
  styleUrls: ["./checkpoint-groups.component.scss"],
})
export class CheckpointGroupsComponent {
  @Input()
  checkpointGroups: CheckpointGroup[];
  @Input()
  selectedCheckpointGroup: CheckpointGroup;
  @Output()
  checkpointGroupSelected = new EventEmitter<UrlParams>();

  constructor() {}

  onSelectOptionChange(checkpointGroupId: number) {
    this.checkpointGroupSelected.emit({ checkpointGroupId });
  }
}
