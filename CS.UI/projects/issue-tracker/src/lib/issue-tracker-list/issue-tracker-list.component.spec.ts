import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueTrackerListComponent } from './issue-tracker-list.component';

describe('IssueTrackerListComponent', () => {
  let component: IssueTrackerListComponent;
  let fixture: ComponentFixture<IssueTrackerListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IssueTrackerListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IssueTrackerListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
