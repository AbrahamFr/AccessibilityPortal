import { Directive, ElementRef, HostListener, Renderer2 } from "@angular/core";

@Directive({
  selector: "[toggleList]"
})
export class ToggleListDirective {
  constructor(private elementRef: ElementRef, private renderer: Renderer2) {}

  @HostListener("document:click", ["$event"]) onClick(e) {
    const element = this.elementRef.nativeElement;
    if (element && element.id == e.target.id) {
      const isExpanded = element.classList.contains("expand");
      if (isExpanded) {
        this.renderer.removeClass(element, "expand");
        this.renderer.setAttribute(element, "aria-expanded", "false");
      } else {
        this.renderer.addClass(element, "expand");
        this.renderer.setAttribute(element, "aria-expanded", "true");
      }
    }
  }
}