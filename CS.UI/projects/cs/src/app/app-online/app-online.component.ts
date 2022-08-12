import { Component, OnInit } from '@angular/core';
import { Observable } from "rxjs/Observable";
import 'rxjs/Rx';

@Component({
  selector: 'app-app-online',
  templateUrl: './app-online.component.html',
  styleUrls: ['./app-online.component.scss']
})
export class AppOnlineComponent implements OnInit {
  online$: Observable<boolean>;

  constructor() { 
    this.online$ = Observable.merge(
      Observable.of(navigator.onLine),
      Observable.fromEvent(window, 'online').mapTo(true),
      Observable.fromEvent(window, 'offline').mapTo(false)
    )
  }

  ngOnInit() {
  }

}
