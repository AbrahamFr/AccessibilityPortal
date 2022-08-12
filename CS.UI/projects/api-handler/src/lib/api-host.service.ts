import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ApiHostService {

  constructor() { }

  public getAPIHostName()
  {
    //return window.location.protocol + "//" + window.location.hostname + "/CS";
  }
}
