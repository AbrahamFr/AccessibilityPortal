import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { PagePerformanceComponent } from "./page-performance.component";

describe("HScanComponent", () => {
  let component: PagePerformanceComponent;
  let fixture: ComponentFixture<PagePerformanceComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [PagePerformanceComponent]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PagePerformanceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
