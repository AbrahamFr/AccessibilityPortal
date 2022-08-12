import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrenceFilterHtmlElementComponent } from './occurrence-filter-html-element.component';

describe('OccurrenceFilterHtmlElementComponent', () => {
  let component: OccurrenceFilterHtmlElementComponent;
  let fixture: ComponentFixture<OccurrenceFilterHtmlElementComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrenceFilterHtmlElementComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrenceFilterHtmlElementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
