import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ApiHandlerComponent } from './api-handler.component';

describe('ApiHandlerComponent', () => {
  let component: ApiHandlerComponent;
  let fixture: ComponentFixture<ApiHandlerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ApiHandlerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ApiHandlerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
