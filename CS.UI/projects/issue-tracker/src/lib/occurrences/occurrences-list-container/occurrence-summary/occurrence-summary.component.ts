import { Component, Input } from "@angular/core";
import { InteractionsService } from "interactions";

@Component({
  selector: "app-occurrence-summary",
  templateUrl: "./occurrence-summary.component.html",
  styleUrls: ["./occurrence-summary.component.scss"],
})
export class OccurrenceSummaryComponent {
  @Input()
  occurrenceSummary;

  constructor(private interactionsService: InteractionsService) {}
  useCInvStyles = this.interactionsService.useCInvStyles;
}
