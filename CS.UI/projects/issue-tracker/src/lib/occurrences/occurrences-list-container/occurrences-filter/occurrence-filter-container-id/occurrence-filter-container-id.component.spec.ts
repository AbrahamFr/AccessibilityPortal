import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrenceFilterContainerIdComponent } from './occurrence-filter-container-id.component';

describe('OccurrenceFilterContainerIdComponent', () => {
  let component: OccurrenceFilterContainerIdComponent;
  let fixture: ComponentFixture<OccurrenceFilterContainerIdComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrenceFilterContainerIdComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrenceFilterContainerIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
