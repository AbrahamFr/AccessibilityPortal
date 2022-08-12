import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrencesFilterComponent } from './occurrences-filter.component';

describe('OccurrencesFilterComponent', () => {
  let component: OccurrencesFilterComponent;
  let fixture: ComponentFixture<OccurrencesFilterComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrencesFilterComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrencesFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
