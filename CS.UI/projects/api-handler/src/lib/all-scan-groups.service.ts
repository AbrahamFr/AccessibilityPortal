import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BehaviorSubject } from "rxjs";
import { map, publishReplay, startWith, refCount } from "rxjs/operators";
import { ScanGroup } from "cs-core";

@Injectable({
  providedIn: "root",
})
export class AllScanGroupsService {
  readonly scanGroupActive$ = new BehaviorSubject<boolean>(true);
  readonly hasScanGroups$ = new BehaviorSubject<boolean>(true);

  private allScanGroupsUrl = "rest/ScanGroup/allScangroups";

  constructor(private http: HttpClient) {}

  readonly allScanGroupsList = this.http
    .get<ScanGroup[]>(this.allScanGroupsUrl)
    .pipe(
      startWith(0),
      map((response) => response as ScanGroup[]),
      publishReplay(1),
      refCount()
    );

  getScanGroupActive(): boolean {
    return this.scanGroupActive$.value;
  }
  updateScanGroupActive(value: boolean): void {
    this.scanGroupActive$.next(value);
  }

  getHasScanGroupActiveValue(): boolean {
    return this.hasScanGroups$.value;
  }

  checkOnUpdateForScanGroup() {
    if (!this.hasScanGroups$.value) {
      this.scanGroupActive$.next(false);
    }
  }
  switchScanGroupActive() {
    this.scanGroupActive$.next(!this.scanGroupActive$.value);
  }
}
