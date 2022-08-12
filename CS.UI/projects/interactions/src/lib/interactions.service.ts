import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root",
})
export class InteractionsService {
  constructor() {}

  private _useCInvStyles: boolean = false;

  public get useCInvStyles() {
    if (!this._useCInvStyles) {
      const localStorageValue = localStorage.getItem("useCInvStyles");
      if (localStorageValue === "true") {
        this._useCInvStyles = true;
      }
    }
    return this._useCInvStyles;
  }

  public set useCInvStyles(value: boolean) {
    localStorage.setItem("useCInvStyles", value === true ? "true" : "false");
    this._useCInvStyles = value;
  }
}
