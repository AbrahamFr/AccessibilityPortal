import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { map, publishReplay, startWith, refCount } from "rxjs/operators";
import { Scan } from "./types";

@Injectable({
  providedIn: "root",
})
export class ScansService {
  private scansListUrl = "rest/Scan/scansList";

  constructor(private http: HttpClient) {}

  readonly scansList = this.http.get<Scan[]>(this.scansListUrl).pipe(
    startWith(0),
    map((response) => response as Scan[]),
    publishReplay(1),
    refCount()
  );
}
