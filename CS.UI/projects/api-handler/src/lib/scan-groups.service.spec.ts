import { TestBed } from '@angular/core/testing';

import { ScanGroupsService } from './scan-groups.service';

describe('ScanGroupsService', () => {
  let service: ScanGroupsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ScanGroupsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
