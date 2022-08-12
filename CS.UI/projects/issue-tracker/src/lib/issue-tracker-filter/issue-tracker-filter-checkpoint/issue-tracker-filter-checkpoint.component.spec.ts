import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueTrackerFilterCheckpointComponent } from './issue-tracker-filter-checkpoint.component';

describe('IssueTrackerFilterCheckpointComponent', () => {
  let component: IssueTrackerFilterCheckpointComponent;
  let fixture: ComponentFixture<IssueTrackerFilterCheckpointComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IssueTrackerFilterCheckpointComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IssueTrackerFilterCheckpointComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
