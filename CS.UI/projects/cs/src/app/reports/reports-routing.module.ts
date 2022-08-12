import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { ReportsComponent } from "./reports.component";
import { PageNotFoundComponent } from "../page-not-found/page-not-found.component";

const reportsRoutes: Routes = [
  {
    path: "",
    component: ReportsComponent,
    children: [
      {
        path: "Trend",
        loadChildren: () =>
          import("./trend/trend.module").then((mod) => mod.TrendModule),
      },
      {
        path: "Audits",
        loadChildren: () =>
          import("./audits/audits.module").then((mod) => mod.AuditsModule),
      },
      {
        path: "IssueTracker",
        loadChildren: () =>
          import("issue-tracker").then((mod) => mod.IssueTrackerModule),
      },
      {
        path: "**",
        component: PageNotFoundComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(reportsRoutes)],
  exports: [RouterModule],
})
export class ReportsRoutingModule {}
