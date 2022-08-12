import { Directive, ElementRef, Input, OnChanges } from "@angular/core";

@Directive({
  selector: "[setFocusNext]"
})
export class SetFocusNextDirective implements OnChanges {
  @Input() setFocusNext: boolean = false;
  @Input() focusDelay: number = 0;

  constructor(private elementRef: ElementRef) {}

  ngOnChanges() {
    this.checkFocus();
  }

  private checkFocus() {
    if (this.setFocusNext) {
      let checkFocusTimeoutHandle: number;
      const focus = () => {
        this.elementRef.nativeElement.focus();
      };
      // Even without a delay, we wait for the next JavaScript tick
      // to avoid causing changes on parent components that have
      // already been checked on this change detection cycle.
      checkFocusTimeoutHandle = setTimeout(focus, this.focusDelay) as any;
    }
  }
}
