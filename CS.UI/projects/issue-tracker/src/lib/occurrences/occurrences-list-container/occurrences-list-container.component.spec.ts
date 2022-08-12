import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OccurrencesListContainerComponent } from './occurrences-list-container.component';

describe('OccurrencesListContainerComponent', () => {
  let component: OccurrencesListContainerComponent;
  let fixture: ComponentFixture<OccurrencesListContainerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OccurrencesListContainerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrencesListContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
