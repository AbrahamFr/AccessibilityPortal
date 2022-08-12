import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueTrackerSortComponent } from './issue-tracker-sort.component';

describe('IssueTrackerSortComponent', () => {
  let component: IssueTrackerSortComponent;
  let fixture: ComponentFixture<IssueTrackerSortComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IssueTrackerSortComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IssueTrackerSortComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
