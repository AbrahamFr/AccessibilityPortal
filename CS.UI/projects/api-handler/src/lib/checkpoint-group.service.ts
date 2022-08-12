import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { map, publishReplay, startWith, refCount, tap } from "rxjs/operators";
import { BehaviorSubject, Observable } from "rxjs";
import { CheckpointGroup } from "cs-core";

@Injectable({
  providedIn: "root",
})
export class CheckpointGroupService {
  readonly checkpointGroups$ = new BehaviorSubject<CheckpointGroup[]>([]);
  private checkpointGroupsUrl = "rest/CheckpointGroup/checkpointGroupList";
  private checkpointGroupsByUrl = "rest/CheckpointGroup/checkpointGroupsBy";

  constructor(private http: HttpClient) {}

  readonly checkpointGroupList = this.http
    .get<CheckpointGroup[]>(this.checkpointGroupsUrl)
    .pipe(
      startWith(0),
      map((response) => response as CheckpointGroup[]),
      publishReplay(1),
      refCount()
    );

  getCheckpointGroupListBy(param?: any): Observable<CheckpointGroup[]> {
    return this.http
      .get<CheckpointGroup[]>(this.checkpointGroupsByUrl, { params: param })
      .pipe(
        startWith([]),
        map((response) => response as CheckpointGroup[]),
        publishReplay(1),
        refCount(),
        tap((groups) => this.checkpointGroups$.next(groups))
      );
  }
}
