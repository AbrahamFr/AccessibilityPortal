import { Component, OnInit, Input } from '@angular/core';
import { RoutePathService } from '../../../routing/route-path.service';

@Component({
  selector: 'app-sub-nav',
  templateUrl: './sub-nav.component.html',
  styleUrls: ['./sub-nav.component.scss']
})
export class SubNavComponent implements OnInit {
  @Input()
  homePath: string;
  @Input()
  orgVirtualDir: string | null
  @Input()
  childRoute: string;

  showReports: boolean;

  constructor(
    private routePathService: RoutePathService
  ) { }

  ngOnInit() {
    this.chooseWhichSubNav();
  }

  chooseWhichSubNav() {
    this.showReports = this.routePathService.checkRouteForPath('reports');
  }
}
