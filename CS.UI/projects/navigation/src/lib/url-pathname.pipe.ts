import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: "urlPathname" })
export class UrlPathnamePipe implements PipeTransform {
  transform(value: string) {
    if (typeof value !== "string") {
      return value;
    }
    const url = new URL(value)
    return url.pathname;
  }
}