import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrenceFilterPageTitleComponent } from './occurrence-filter-page-title.component';

describe('OccurrenceFilterPageTitleComponent', () => {
  let component: OccurrenceFilterPageTitleComponent;
  let fixture: ComponentFixture<OccurrenceFilterPageTitleComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrenceFilterPageTitleComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrenceFilterPageTitleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
