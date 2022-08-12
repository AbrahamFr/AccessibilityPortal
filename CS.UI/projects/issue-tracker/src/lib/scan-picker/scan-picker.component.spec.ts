import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ScanPickerComponent } from './scan-picker.component';

describe('ScanPickerComponent', () => {
  let component: ScanPickerComponent;
  let fixture: ComponentFixture<ScanPickerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ScanPickerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScanPickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
