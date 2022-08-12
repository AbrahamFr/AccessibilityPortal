import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-header-nav',
  templateUrl: './header-nav.component.html',
  styleUrls: ['./header-nav.component.scss'],
})
export class HeaderNavComponent implements OnInit {
  @Input()
  homePath: string;

  constructor(
  ) { }

  ngOnInit() {
  }

}
