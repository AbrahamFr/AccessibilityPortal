import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrenceFilterKeyAttributeComponent } from './occurrence-filter-key-attribute.component';

describe('OccurrenceFilterKeyAttributeComponent', () => {
  let component: OccurrenceFilterKeyAttributeComponent;
  let fixture: ComponentFixture<OccurrenceFilterKeyAttributeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrenceFilterKeyAttributeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrenceFilterKeyAttributeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
