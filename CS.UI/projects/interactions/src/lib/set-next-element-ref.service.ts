import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SetNextElementRefService {
  readonly setNextElementRef$ = new BehaviorSubject<HTMLElement | null>(null);

  constructor() { }
}
