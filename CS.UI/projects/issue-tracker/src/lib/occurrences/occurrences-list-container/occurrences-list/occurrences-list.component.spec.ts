import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrencesListComponent } from './occurrences-list.component';

describe('OccurrencesListComponent', () => {
  let component: OccurrencesListComponent;
  let fixture: ComponentFixture<OccurrencesListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrencesListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrencesListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
