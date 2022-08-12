import { Component, OnInit } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { Router, NavigationStart } from "@angular/router";
import { filter, pairwise, startWith } from "rxjs/operators";
import { UrlParamsService } from "navigation";
import { AuthMode, AuthenticationService } from 'authentication';

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"],
})
export class AppComponent implements OnInit {
  private authMode: AuthMode = AuthMode.AuthWebForms;

  ngOnInit()
  {
    this.authService.setAuthMode(this.authMode);
  }  
  
  constructor(
    private readonly translate: TranslateService,
    private router: Router,
    private urlParamService: UrlParamsService,
    private authService: AuthenticationService
  ) {
    translate.addLangs(["en"]),
      translate.setDefaultLang("en"),
      translate.use("en");

    this.router.events
      .pipe(
        startWith(null),
        filter((e) => e instanceof NavigationStart),
        pairwise()
      )
      .subscribe((e: any[]) => {
        const previousUrl = e[0].url.split(";")[0];
        const currentUrl = e[1].url.split(";")[0];
        if (previousUrl !== currentUrl) {
          this.urlParamService.previousUrl$.next(previousUrl);
        }
      });
  }  
}
