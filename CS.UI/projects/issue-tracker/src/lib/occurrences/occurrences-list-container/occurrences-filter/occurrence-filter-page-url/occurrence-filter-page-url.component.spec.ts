import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrenceFilterPageUrlComponent } from './occurrence-filter-page-url.component';

describe('OccurrenceFilterPageUrlComponent', () => {
  let component: OccurrenceFilterPageUrlComponent;
  let fixture: ComponentFixture<OccurrenceFilterPageUrlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrenceFilterPageUrlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrenceFilterPageUrlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
