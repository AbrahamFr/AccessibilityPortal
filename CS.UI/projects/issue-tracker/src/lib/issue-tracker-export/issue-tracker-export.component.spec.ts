import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueTrackerExportComponent } from './issue-tracker-export.component';

describe('IssueTrackerExportComponent', () => {
  let component: IssueTrackerExportComponent;
  let fixture: ComponentFixture<IssueTrackerExportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IssueTrackerExportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IssueTrackerExportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
