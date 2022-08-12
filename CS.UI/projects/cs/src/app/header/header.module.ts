import { NgModule } from "@angular/core";
import { SharedModule } from "../shared/shared.module";

import { HeaderComponent } from "./header.component";
import { HeaderNavComponent } from "./header-nav/header-nav.component";
import { MenuDropdownComponent } from "./menu-dropdown/menu-dropdown.component";
import { SubNavComponent } from "./sub-nav/sub-nav.component";
import { EmptyComponent } from "./sub-nav/empty/empty.component";
import { ReportsComponent } from "./sub-nav/reports/reports.component";
import { HeaderRoutingModule } from "./header-routing.module";
import { BreadcrumbNavComponent } from "./breadcrumb-nav/breadcrumb-nav.component";
import { BreadcrumbComponent } from "./breadcrumb-nav/breadcrumb/breadcrumb.component";
import { IssueTrackerModule } from "issue-tracker";

@NgModule({
  declarations: [
    HeaderComponent,
    HeaderNavComponent,
    MenuDropdownComponent,
    SubNavComponent,
    EmptyComponent,
    ReportsComponent,
    BreadcrumbNavComponent,
    BreadcrumbComponent,
  ],
  imports: [SharedModule, HeaderRoutingModule, IssueTrackerModule],
  exports: [HeaderComponent],
})
export class HeaderModule {}
