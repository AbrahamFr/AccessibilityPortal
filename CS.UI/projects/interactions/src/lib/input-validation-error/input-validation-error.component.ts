import { Component, Input, OnInit } from "@angular/core";
import { InteractionsService } from "../interactions.service";

@Component({
  selector: "interactions-input-validation-error",
  templateUrl: "./input-validation-error.component.html",
  styleUrls: ["./input-validation-error.component.scss"],
})
export class InputValidationErrorComponent implements OnInit {
  @Input()
  errorMessage: string;
  @Input()
  param?: any;
  @Input()
  styles?: any | null = {};

  constructor(private interactionsService: InteractionsService) {}
  useCInvStyles = this.interactionsService.useCInvStyles;

  ngOnInit() {}
}
