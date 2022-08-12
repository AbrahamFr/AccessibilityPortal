import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ScanHistoryChartComponent } from './scan-history-chart.component';

describe('ScanHistoryChartComponent', () => {
  let component: ScanHistoryChartComponent;
  let fixture: ComponentFixture<ScanHistoryChartComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ScanHistoryChartComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScanHistoryChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
