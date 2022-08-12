import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, publishReplay, refCount, startWith } from 'rxjs/operators';
import { TimeZone } from './types';

@Injectable({
  providedIn: 'root'
})
export class TimeZoneService {
  private allTimeZonesUrl = "rest/TimeZone/";

  constructor(private http: HttpClient) { }

  readonly allTimeZonesList = this.http.get<TimeZone[]>(this.allTimeZonesUrl).pipe(
    startWith(0),
    map((response) => response as TimeZone[]),
    publishReplay(1),
    refCount()
  );  
}
