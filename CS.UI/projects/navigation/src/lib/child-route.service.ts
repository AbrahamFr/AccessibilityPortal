import { Location } from "@angular/common";
import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root"
})
export class ChildRouteService {
  supportedChildPaths = {
    reports: ["/Trend", "/Audits", "/IssueTracker", "/IssueTracker/Occurrences", "/auditresults", "/auditresults/Occurrences"]
  };
  baseIndex: number = -1;

  constructor(private location: Location) {}

  getChildRoute(): string {
    const url = this.location
      .path()
      .split("?")[0];
    const childPath = url.split("/");
    if (this.isChildPathSupported(childPath.map(p => p.toLowerCase()))) {
      const childRoute = childPath.splice(this.baseIndex, childPath.length - 1);
      return childRoute.join("/");
    }
    return "";
  }

  isChildPathSupported(childPathSegment: string[]): boolean {
    let supported = false;
    let slicedPath = "";

    for (let path of Object.keys(this.supportedChildPaths)) {
      this.baseIndex = childPathSegment.indexOf(path.toLowerCase());
      if (this.baseIndex !== -1) {
        childPathSegment.slice(this.baseIndex + 1).forEach(p => {
          if (p) {
            return (slicedPath += "/" + p);
          }
        });
        this.supportedChildPaths[path].find(r => {
          if (r.toLowerCase() == slicedPath) {
            supported = true;
          }
        });
      }
    }
    return supported;
  }
}
