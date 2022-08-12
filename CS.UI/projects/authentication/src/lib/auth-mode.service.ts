import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthMode } from './types';

@Injectable({
  providedIn: 'root'
})
export class AuthModeService {
    private readonly authModeAPI$: BehaviorSubject<AuthMode> = new BehaviorSubject<AuthMode>(AuthMode.AuthWebForms);
    
  constructor() { }

  public get authenticationMode$() {
    return this.authModeAPI$.asObservable();
  }

  public get authenticationMode() {
    return this.authModeAPI$.value;
  }  

}
