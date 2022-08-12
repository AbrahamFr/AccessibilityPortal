import { BehaviorSubject } from "rxjs";
import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root"
})
export class JwtStoreService {
  private readonly authToken$: BehaviorSubject<
    string | null
  > = new BehaviorSubject<string | null>(null);

  public receiveJwtToken(authToken: string | null) {
    this.authToken$.next(authToken);
  }
  public get authenticationToken$() {
    return this.authToken$.asObservable();
  }
  // If we want to remove Local Storage in the future, we can use this to avoid resubscribing to Claims observable.
  public get authenticationToken() {
    return this.authToken$.value;
  }
}
