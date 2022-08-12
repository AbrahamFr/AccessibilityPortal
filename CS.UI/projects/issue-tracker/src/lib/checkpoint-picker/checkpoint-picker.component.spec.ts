import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckpointPickerComponent } from './checkpoint-picker.component';

describe('CheckpointPickerComponent', () => {
  let component: CheckpointPickerComponent;
  let fixture: ComponentFixture<CheckpointPickerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckpointPickerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckpointPickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
