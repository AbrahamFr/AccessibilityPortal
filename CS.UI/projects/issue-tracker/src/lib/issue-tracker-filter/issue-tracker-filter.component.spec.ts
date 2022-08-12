import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueTrackerFilterComponent } from './issue-tracker-filter.component';

describe('IssueTrackerFilterComponent', () => {
  let component: IssueTrackerFilterComponent;
  let fixture: ComponentFixture<IssueTrackerFilterComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IssueTrackerFilterComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IssueTrackerFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
