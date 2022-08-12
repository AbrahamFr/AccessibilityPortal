import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckpointPerformanceGaugeComponent } from './checkpoint-performance-gauge.component';

describe('CheckpointPerformanceGaugeComponent', () => {
  let component: CheckpointPerformanceGaugeComponent;
  let fixture: ComponentFixture<CheckpointPerformanceGaugeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckpointPerformanceGaugeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckpointPerformanceGaugeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
