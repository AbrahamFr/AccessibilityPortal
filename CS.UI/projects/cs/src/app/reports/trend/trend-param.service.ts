import { Injectable } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { BehaviorSubject } from "rxjs";
import { UrlParamsService } from "navigation";
import { TrendParams } from "cs-core";

@Injectable({
  providedIn: "root",
})
export class TrendParamService {
  readonly trendParams$ = new BehaviorSubject<TrendParams>({});
  constructor(
    private urlParamService: UrlParamsService,
    private route: ActivatedRoute
  ) {}

  checkInitialState() {
    const queryParams = this.route.snapshot.queryParams;
    const previousUrl = this.urlParamService.previousUrl$.value;
    const isNavPopstate = this.urlParamService.getIsNavPopstate();

    if (Object.keys(queryParams).length > 0 && !previousUrl) {
      if (queryParams["scanGroupId"]) {
        this.trendParams$.next(queryParams);
        this.urlParamService.updateUrlParams(queryParams);
      } else {
        this.setInitialParams();
      }
    } else if (isNavPopstate) {
      this.urlParamService.updateUrlParams(this.trendParams$.value);
      this.urlParamService.closeIsNavePopstate();
    } else {
      this.setInitialParams();
    }
  }

  setInitialParams() {
    this.trendParams$.next({});
  }
}
