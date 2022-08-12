import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TopFailuresComponent } from './top-failures.component';

describe('TopFailuresComponent', () => {
  let component: TopFailuresComponent;
  let fixture: ComponentFixture<TopFailuresComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TopFailuresComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TopFailuresComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
