import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ScanChartComponent } from './scan-chart.component';

describe('ScanChartComponent', () => {
  let component: ScanChartComponent;
  let fixture: ComponentFixture<ScanChartComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ScanChartComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScanChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
