import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrenceSummaryComponent } from './occurrence-summary.component';

describe('OccurrecesSummaryComponent', () => {
  let component: OccurrenceSummaryComponent;
  let fixture: ComponentFixture<OccurrenceSummaryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrenceSummaryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrenceSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
