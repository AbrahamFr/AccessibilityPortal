import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { ApiHandlerErrorGuardComponent } from "./api-handler-error-guard.component";

describe("AppErrorGuardComponent", () => {
  let component: ApiHandlerErrorGuardComponent;
  let fixture: ComponentFixture<ApiHandlerErrorGuardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ApiHandlerErrorGuardComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ApiHandlerErrorGuardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
