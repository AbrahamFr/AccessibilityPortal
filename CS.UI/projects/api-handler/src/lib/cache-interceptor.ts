import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler } from '@angular/common/http';

/**
   * Intercepts HTTP requests and adds no-cache headers to ensure that the URI resource returned is
   * always the most current, up to date version
   * Resolve Cache Issue for IIS
   * @public
   * @method intercept
   * @param {HttpRequest}   httpRequest   The Outgoing HTTP request to be intercepted
   * @param {HttpHandler}   nextRequest   Transforms an HttpRequest object into a stream of HttpEvents
   * @returns {Observable<HttpEvent<any>>}
   */
@Injectable()
export class CacheInterceptor implements HttpInterceptor {

  intercept(request: HttpRequest<any>, next: HttpHandler) {
    if (request.method === 'GET') {
      const customRequest = request.clone({
        headers: request.headers.set('Cache-Control', 'no-cache')
          .set('Pragma', 'no-cache')
      });
      return next.handle(customRequest);
    }

    return next.handle(request);
  }
}