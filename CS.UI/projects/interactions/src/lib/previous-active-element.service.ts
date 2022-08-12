import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PreviousActiveElementService {
  readonly previousActiveElement$ = new BehaviorSubject<HTMLElement | null>(null);

  constructor() { }

  setFocus() {
    const element = this.previousActiveElement$.value;
    if (element) {
      element.focus();
    }
  }
}
