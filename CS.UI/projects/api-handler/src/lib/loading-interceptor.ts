import { Injectable } from "@angular/core";
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
} from "@angular/common/http";
import { Observable } from "rxjs";
import { LoadingService } from "./loading.service";
import { tap } from "rxjs/operators";

@Injectable({
  providedIn: "root",
})
export class LoadingInterceptor implements HttpInterceptor {
  constructor(private loadingService: LoadingService) {}
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      tap({
        next: (_) => {
          // Turn spinner on
          this.loadingService.loading$.next(true);
        },
        complete: () => {
          // Turn spinner off
          this.loadingService.loading$.next(false);
        },
      })
    );
  }
}
