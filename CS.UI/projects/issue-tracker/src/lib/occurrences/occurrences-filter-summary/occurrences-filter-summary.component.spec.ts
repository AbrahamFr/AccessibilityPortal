import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrencesFilterSummaryComponent } from './occurrences-filter-summary.component';

describe('OccurrencesFilterSummaryComponent', () => {
  let component: OccurrencesFilterSummaryComponent;
  let fixture: ComponentFixture<OccurrencesFilterSummaryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrencesFilterSummaryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrencesFilterSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
