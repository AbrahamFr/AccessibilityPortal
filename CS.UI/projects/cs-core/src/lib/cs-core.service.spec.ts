import { TestBed } from "@angular/core/testing";

import { CsCoreService } from "./cs-core.service";

describe("CsCoreService", () => {
  let service: CsCoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CsCoreService);
  });

  it("should be created", () => {
    expect(service).toBeTruthy();
  });
});
