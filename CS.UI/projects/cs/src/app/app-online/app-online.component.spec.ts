import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AppOnlineComponent } from './app-online.component';

describe('AppOnlineComponent', () => {
  let component: AppOnlineComponent;
  let fixture: ComponentFixture<AppOnlineComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AppOnlineComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AppOnlineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
