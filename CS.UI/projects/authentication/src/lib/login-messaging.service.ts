import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class LoginMessagingService {
  constructor() {}

  private readonly _loginErrors$: BehaviorSubject<
    string | null
  > = new BehaviorSubject<string | null>(null);

  public get loginErrors$() {
    return this._loginErrors$.asObservable();
  }

  public addLoginErrors(loginError: string | null) {
    return this._loginErrors$.next(loginError);
  }
}
