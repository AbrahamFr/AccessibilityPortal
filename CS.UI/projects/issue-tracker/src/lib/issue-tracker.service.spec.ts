import { TestBed } from '@angular/core/testing';

import { IssueTrackerService } from './issue-tracker.service';

describe('IssueTrackerService', () => {
  let service: IssueTrackerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(IssueTrackerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
