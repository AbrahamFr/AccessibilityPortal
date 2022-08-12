import { Directive, Input, ElementRef, AfterViewInit } from "@angular/core";

@Directive({
  selector: "[focus]"
})
export class FocusDirective implements AfterViewInit {
  @Input("focus") boolean;

  constructor(private element: ElementRef) {}

  ngAfterViewInit() {
    setTimeout(() => {
      this.element.nativeElement.focus();
    }, 0);
  }
}
