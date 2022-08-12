import { Component, OnInit, Input, AfterContentChecked } from '@angular/core';

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss']
})
export class BreadcrumbComponent implements OnInit, AfterContentChecked {
  @Input()
  homePath: string;
  @Input()
  orgVirtualDir: string | null
  @Input()
  childRoute: string;

  breadCrumbChilds: string[];
  activeChild: string;
  topLevelNavPath: string;

  constructor(
  ) { }

  ngOnInit() {
  }

  ngAfterContentChecked(): void {
    this.breadCrumbChilds = this.childRoute.split("/");
    this.activeChild = this.breadCrumbChilds[this.breadCrumbChilds.length - 1];
    this.topLevelNavPath = `${this.orgVirtualDir}/${this.breadCrumbChilds[0]}`
  }


}
