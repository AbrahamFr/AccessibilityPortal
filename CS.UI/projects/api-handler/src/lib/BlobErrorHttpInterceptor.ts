import { Injectable } from "@angular/core";
import { HttpHandler, HttpInterceptor, HttpRequest, HttpEvent, HttpErrorResponse } from "@angular/common/http";
import { Observable } from "rxjs";
import { catchError } from "rxjs/operators";

@Injectable()
export class BlobErrorHttpInterceptor implements HttpInterceptor {
    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError(error => {
                if (error instanceof HttpErrorResponse && error.error instanceof Blob && error.error.type === "application/json") {
                    // https://github.com/angular/angular/issues/19888
                    // When request of type Blob, the error is also in Blob instead of object of the json data
                    return new Promise<any>((resolve, reject) => {
                        let reader = new FileReader();
                        reader.onload = (e: Event) => {
                            try {
                                const errmsg = JSON.parse((<any>e.target).result);
                                reject(new HttpErrorResponse({
                                    error: errmsg,
                                    headers: error.headers,
                                    status: error.status,
                                    statusText: error.statusText
                                }));
                            } catch (e) {
                                console.log(e);
                                reject(error);
                            }
                        };
                        reader.onerror = (e) => {
                            console.log(e);
                            reject(error);
                        };
                        reader.readAsText(error.error);
                    });
                }
                throw(error);
            })
        );
    }
}