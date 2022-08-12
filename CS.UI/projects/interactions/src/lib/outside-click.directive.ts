import { Directive, ElementRef, EventEmitter, Input, HostListener, Output } from "@angular/core";

@Directive({
  selector: "[outsideClick]"
})
export class OutsideClickDirective {
  @Output() outsideClick = new EventEmitter<HTMLElement>();

  constructor(private elementRef: ElementRef) {}

  @HostListener("document:click", ["$event.target"])
  public onClick(target) {
    const clickedInside = this.elementRef.nativeElement.contains(target);
    if (!clickedInside) {
        this.outsideClick.emit(target);
    }
  }
}