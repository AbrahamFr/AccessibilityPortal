import { TestBed } from '@angular/core/testing';

import { RoutePathService } from './route-path.service';

describe('RoutePathService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: RoutePathService = TestBed.get(RoutePathService);
    expect(service).toBeTruthy();
  });
});
