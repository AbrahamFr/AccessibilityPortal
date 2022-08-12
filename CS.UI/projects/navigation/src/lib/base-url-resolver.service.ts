import { Injectable, Inject } from "@angular/core";
import { DOCUMENT } from "@angular/common";

@Injectable({
  providedIn: "root"
})
export class BaseUrlResolver {
  constructor(@Inject(DOCUMENT) private document: Document) {}

  get baseUrl() {
    return this.document.getElementsByTagName("base")[0].href;
  }
}
