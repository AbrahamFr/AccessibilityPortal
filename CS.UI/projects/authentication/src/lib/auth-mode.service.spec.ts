import { TestBed } from '@angular/core/testing';

import { AuthModeService } from './auth-mode.service';

describe('AuthModeService', () => {
  let service: AuthModeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthModeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
