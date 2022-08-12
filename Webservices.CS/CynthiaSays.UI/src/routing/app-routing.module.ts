import { NgModule, InjectionToken } from "@angular/core";
import { RouterModule, Routes, ActivatedRouteSnapshot } from "@angular/router";
import { TestSiteComponent } from "src/app/test-site/test-site.component";
import { AboutComponent } from "src/app/about/about.component";
import { LearnMoreComponent } from "src/app/learn-more/learn-more.component";
import { TermsOfUseComponent } from "src/app/terms-of-use/terms-of-use.component";

const externalUrlProvider = new InjectionToken("externalUrlRedirectResolver");
const routes: Routes = [
  {
    path: "",
    component: TestSiteComponent
  },
  {
    path: "LearnMore",
    component: LearnMoreComponent
  },
  {
    path: "About",
    component: AboutComponent
  },
  {
    path: "TermsOfUse",
    component: TermsOfUseComponent
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      useHash: false,
      anchorScrolling: "enabled",
      initialNavigation: "enabled"
    })
  ],
  exports: [RouterModule],
  providers: [
    {
      provide: externalUrlProvider,
      useValue: (routeSnapshot: ActivatedRouteSnapshot) => {
        const externalUrl = routeSnapshot.paramMap.get("externalUrl");
        window.open(externalUrl as string, "_self");
      }
    }
  ]
})
export class AppRoutingModule {}
