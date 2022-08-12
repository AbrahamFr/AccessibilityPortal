import { Component, Input, OnInit, ViewChildren } from "@angular/core";
import {
  END,
  ENTER,
  HOME,
  LEFT_ARROW,
  RIGHT_ARROW,
  SPACE,
  TAB
} from "@angular/cdk/keycodes";
import { BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";

const supportedKeyCodes = {
  RIGHT_ARROW,
  LEFT_ARROW,
  SPACE,
  ENTER,
  TAB,
  HOME,
  END
};

@Component({
  selector: "app-top-failures",
  templateUrl: "./top-failures.component.html",
  styleUrls: ["./top-failures.component.scss"]
})
export class TopFailuresComponent implements OnInit {
  @ViewChildren("topFailures")
  topFailures;
  @Input()
  scanGroupId: string;
  @Input()
  hasScanGroups: boolean;

  constructor() {}

  isPageFailuresActive: boolean = true;
  readonly activePageFailures = new BehaviorSubject<boolean>(
    this.isPageFailuresActive
  );

  pageFailuresVisibility = this.activePageFailures.pipe(
    map(shouldShow => (shouldShow ? true : false))
  );

  ngOnInit() {}

  onClickTopFailures(evt: MouseEvent) {
    const topFailuresEls = this.topFailures.toArray();
    const currentIndex = topFailuresEls.findIndex(t =>
      t.nativeElement.contains(evt.target)
    );

    if (this.isPageFailuresActive && currentIndex === 1) {
      topFailuresEls[currentIndex].nativeElement.focus();
      this.updateActiveTable();
    }

    if (!this.isPageFailuresActive && currentIndex === 0) {
      topFailuresEls[currentIndex].nativeElement.focus();
      this.updateActiveTable();
    }
  }

  supportedKeyCode(keyCode: number): boolean {
    return Object.values(supportedKeyCodes).includes(keyCode);
  }

  onKeydownTopFailures(evt: KeyboardEvent | any) {
    const keyCode = evt.which || evt.keyCode;
    const shiftKey = evt.shiftKey;
    const isKeySupported = this.supportedKeyCode(keyCode);
    
    if (isKeySupported) {
      const topFailuresEls = this.topFailures.toArray();
      const currentIndex = topFailuresEls.findIndex(t =>
        t.nativeElement.contains(evt.target)
      );

      if (keyCode === TAB && shiftKey) {
        topFailuresEls[currentIndex].nativeElement.blur();
      }

      if (keyCode === LEFT_ARROW || keyCode === RIGHT_ARROW) {
        const nextActiveTabIndex = currentIndex === 0 ? 1 : 0;
        topFailuresEls[nextActiveTabIndex].nativeElement.focus();
        this.updateActiveTable();
      }
    }
  }

  updateActiveTable() {
    this.isPageFailuresActive = !this.isPageFailuresActive;
    this.activePageFailures.next(this.isPageFailuresActive);
  }
}
