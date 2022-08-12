import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrencesPagesComponent } from './occurrences-pages.component';

describe('OccurrencesPagesComponent', () => {
  let component: OccurrencesPagesComponent;
  let fixture: ComponentFixture<OccurrencesPagesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrencesPagesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrencesPagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
