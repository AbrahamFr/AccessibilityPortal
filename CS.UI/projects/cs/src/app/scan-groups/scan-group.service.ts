import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { map, publishReplay, startWith, refCount } from "rxjs/operators";

import { ScanGroup } from "cs-core";

@Injectable({
  providedIn: "root",
})
export class ScanGroupService {
  private scanGroupsUrl = "rest/ScanGroup/scheduledScanGroups";

  constructor(private http: HttpClient) {}

  readonly scanGroupList = this.http.get<ScanGroup[]>(this.scanGroupsUrl).pipe(
    startWith(0),
    map((response) => response as ScanGroup[]),
    publishReplay(1),
    refCount()
  );
}
