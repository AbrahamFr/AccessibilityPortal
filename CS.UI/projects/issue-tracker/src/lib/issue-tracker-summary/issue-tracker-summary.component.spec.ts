import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueTrackerSummaryComponent } from './issue-tracker-summary.component';

describe('IssueTrackerSummaryComponent', () => {
  let component: IssueTrackerSummaryComponent;
  let fixture: ComponentFixture<IssueTrackerSummaryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IssueTrackerSummaryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IssueTrackerSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
