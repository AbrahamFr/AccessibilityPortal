import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueTrackerComponent } from './issue-tracker.component';

describe('IssueTrackerComponent', () => {
  let component: IssueTrackerComponent;
  let fixture: ComponentFixture<IssueTrackerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IssueTrackerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IssueTrackerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
