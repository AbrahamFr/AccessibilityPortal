import { Component, OnInit } from "@angular/core";
import { NavigationStart, Router } from '@angular/router';
import { TranslateService } from "@ngx-translate/core";
import { AuthenticationService, AuthMode } from "authentication";
import { UrlParamsService } from 'navigation';
import { filter, pairwise, startWith } from 'rxjs/operators';

@Component({
  selector: "cinv-root",
  templateUrl: "./cinv.component.html",
  styleUrls: ["./cinv.component.scss"],
})
export class CinvComponent implements OnInit {
  title = "compliance-investigate";

  private authMode: AuthMode = AuthMode.AuthWebAPI;

  constructor(
    translate: TranslateService,
    private router: Router,
    private urlParamService: UrlParamsService,
    private authService: AuthenticationService
  ) {
    translate.setDefaultLang("en");
    const htmlLang = navigator.language;
    const shortLang = htmlLang.slice(0, 2);
    translate.use(shortLang);

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

  ngOnInit() {
    this.authService.setAuthMode(this.authMode);
  }
}
