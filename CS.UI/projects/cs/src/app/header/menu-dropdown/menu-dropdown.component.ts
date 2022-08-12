import {
  Component,
  OnInit,
  ViewChildren,
  Input,
  AfterContentChecked,
} from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";

import { AuthenticationService } from "authentication";

const supportedKeyCodes = {
  ArrowDown: 40,
  ArrowRight: 39,
  ArrowUp: 38,
  ArrowLeft: 37,
  Space: 32,
  Escape: 27,
  Enter: 13,
  Tab: 9,
};

@Component({
  selector: "app-menu-dropdown",
  templateUrl: "./menu-dropdown.component.html",
  styleUrls: ["./menu-dropdown.component.scss"],
  host: {
    "(document:click)": "onClick($event)",
  },
})
export class MenuDropdownComponent implements OnInit, AfterContentChecked {
  @ViewChildren("dropdownMenuItems")
  dropdownMenuItems;
  @Input()
  homePath: string;
  @Input()
  orgVirtualDir: string | null;
  @Input()
  childRoute: string;

  userName: string | null;
  isAdministrator: boolean;
  activeChild: string;
  activeHelpUrl: string;

  constructor(private authService: AuthenticationService) {}

  isDropdownActive: boolean = false;
  readonly activeDropdown$ = new BehaviorSubject<boolean>(
    this.isDropdownActive
  );

  ngOnInit() {}

  dropdownVisibility = this.activeDropdown$.pipe(
    map((shouldShow) => (shouldShow ? "block" : "none"))
  );

  ngAfterContentChecked() {
    this.initializeData();
  }

  initializeData() {
    const childRouteNodes = this.childRoute.split("/");
    this.activeChild = childRouteNodes[childRouteNodes.length - 1];

    this.userName = localStorage.getItem("userName");
    this.isAdministrator = this.authService.isAdministrator();
    this.activeHelpUrl = this.homePath.concat(
      "/help.aspx?topic=",
      this.activeChild
    );
  }

  onToggleDropdown() {
    this.isDropdownActive = !this.isDropdownActive;
    this.activeDropdown$.next(this.isDropdownActive);
  }

  onClick(evt: MouseEvent) {
    const dropdownMenuEls = this.dropdownMenuItems.toArray();
    const currentIndex = dropdownMenuEls.findIndex((setting) =>
      setting.nativeElement.contains(evt.target)
    );
    if (this.isDropdownActive && currentIndex < 0) {
      this.onToggleDropdown();
    }
  }

  supportedKeyCode(keyCode: number): boolean {
    return Object.values(supportedKeyCodes).includes(keyCode);
  }

  onKeyDown(evt: KeyboardEvent | any) {
    const keyCode = evt.which || evt.keyCode;
    const isKeySupported = this.supportedKeyCode(keyCode);

    if (this.isDropdownActive && isKeySupported) {
      evt.preventDefault();
      const dropdownMenuEls = this.dropdownMenuItems.toArray();
      // const reportsSubNavEls = this.reportsSubNav.toArray();
      const currentIndex = dropdownMenuEls.findIndex((setting) =>
        setting.nativeElement.contains(evt.target)
      );
      const moveUp = currentIndex > 1;
      const moveDown = currentIndex < dropdownMenuEls.length - 1;

      if (keyCode === 9 && evt.shiftKey === true) {
        this.onToggleDropdown();
        dropdownMenuEls[0].nativeElement.focus();
      }

      if (keyCode === 9 && evt.shiftKey === false) {
        this.onToggleDropdown();
        // reportsSubNavEls[0].nativeElement.focus();
      }

      if (keyCode === 40 && moveDown) {
        const nextIndex = currentIndex + 1;
        dropdownMenuEls[nextIndex].nativeElement.focus();
      }
      if (keyCode === 39 || keyCode === 37) {
        this.onToggleDropdown();
        dropdownMenuEls[0].nativeElement.focus();
      }
      if (keyCode === 38 && moveUp) {
        const nextIndex = currentIndex - 1;
        dropdownMenuEls[nextIndex].nativeElement.focus();
      }
      if (keyCode === 27) {
        this.onToggleDropdown();
        dropdownMenuEls[0].nativeElement.focus();
      }
      if (keyCode === 13 && currentIndex > 0) {
        dropdownMenuEls[currentIndex].nativeElement.childNodes[0].click();
      }
      if (
        this.isDropdownActive &&
        currentIndex === 0 &&
        (keyCode === 13 || keyCode === 32)
      ) {
        this.onToggleDropdown();
        dropdownMenuEls[0].nativeElement.focus();
      }
    }
  }

  onPrint() {
    this.onToggleDropdown();
    window.print();
  }
}
