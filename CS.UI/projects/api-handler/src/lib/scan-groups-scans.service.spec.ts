import { TestBed } from '@angular/core/testing';

import { ScanGroupsScansService } from './scan-groups-scans.service';

describe('ScanGroupsScansService', () => {
  let service: ScanGroupsScansService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ScanGroupsScansService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
