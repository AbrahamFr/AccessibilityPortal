import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckpointPerformanceComponent } from './checkpoint-performance.component';

describe('CheckpointPerformanceComponent', () => {
  let component: CheckpointPerformanceComponent;
  let fixture: ComponentFixture<CheckpointPerformanceComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckpointPerformanceComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckpointPerformanceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
