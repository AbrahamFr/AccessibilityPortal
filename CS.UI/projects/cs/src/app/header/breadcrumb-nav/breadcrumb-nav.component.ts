import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-breadcrumb-nav',
  templateUrl: './breadcrumb-nav.component.html',
  styleUrls: ['./breadcrumb-nav.component.scss']
})
export class BreadcrumbNavComponent implements OnInit {
  @Input()
  homePath: string;
  @Input()
  orgVirtualDir: string | null
  @Input()
  childRoute: string;

  constructor(
  ) { }

  ngOnInit() {
  }

}
