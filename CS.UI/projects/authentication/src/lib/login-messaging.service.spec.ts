import { TestBed } from '@angular/core/testing';

import { LoginMessagingService } from './login-messaging.service';

describe('LoginMessagingService', () => {
  let service: LoginMessagingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoginMessagingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
