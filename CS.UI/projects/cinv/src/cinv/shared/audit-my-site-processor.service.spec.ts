import { TestBed } from '@angular/core/testing';

import { AuditMySiteProcessorService } from './audit-my-site-processor.service';

describe('AuditMySiteProcessorService', () => {
  let service: AuditMySiteProcessorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuditMySiteProcessorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
