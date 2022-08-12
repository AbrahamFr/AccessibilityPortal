import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NextActiveElementService {
  readonly nextActiveElement$ = new BehaviorSubject<HTMLElement | null>(null);

  constructor() { }
}
