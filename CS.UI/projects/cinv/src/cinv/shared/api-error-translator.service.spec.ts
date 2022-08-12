import { TestBed } from '@angular/core/testing';

import { ApiErrorTranslatorService } from './api-error-translator.service';

describe('ApiErrorTranslatorService', () => {
  let service: ApiErrorTranslatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ApiErrorTranslatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
