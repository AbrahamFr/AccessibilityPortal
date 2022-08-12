import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PagePerformanceGaugeComponent } from './page-performance-gauge.component';

describe('GaugeChartComponent', () => {
  let component: PagePerformanceGaugeComponent;
  let fixture: ComponentFixture<PagePerformanceGaugeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PagePerformanceGaugeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PagePerformanceGaugeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
