import { NgModule, InjectionToken } from "@angular/core";
import { RouterModule, Routes, ActivatedRouteSnapshot } from "@angular/router";
import { RouteErrorResolver } from "./route-error-resolver";
import { RedirectComponent } from "../app/redirect/redirect.component";
import { LogoutComponent } from "../app/header/logout/logout.component";
import { AuthGuardOrg } from "authentication";
import { InteractionsErrorComponent } from "interactions";

const externalUrlProvider = new InjectionToken("externalUrlRedirectResolver");
const routes: Routes = [
  {
    path: "externalRedirect",
    resolve: {
      url: externalUrlProvider,
    },
    component: RedirectComponent,
  },
  {
    path: ":orgId",
    canActivate: [AuthGuardOrg],
    canActivateChild: [AuthGuardOrg],
    children: [
      {
        path: "Reports",
        loadChildren: () =>
          import("../app/reports/reports.module").then(
            (mod) => mod.ReportsModule
          ),
      },
    ],
  },
  {
    path: ":orgId",
    canActivate: [AuthGuardOrg],
    children: [
      {
        path: "Error",
        component: InteractionsErrorComponent,
        resolve: { message: RouteErrorResolver },
      },
      {
        path: "Logout",
        component: LogoutComponent,
      },
      {
        path: "**",
        redirectTo: "Error",
      },
    ],
  },
  {
    path: "**",
    component: LogoutComponent,
  },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      useHash: false,
      anchorScrolling: "enabled",
    }),
  ],
  exports: [RouterModule],
  providers: [
    {
      provide: externalUrlProvider,
      useValue: (routeSnapshot: ActivatedRouteSnapshot) => {
        const externalUrl = routeSnapshot.paramMap.get("externalUrl");
        window.open(externalUrl as string, "_self");
      },
    },
    RouteErrorResolver,
  ],
})
export class AppRoutingModule {}
