import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { ScanPerformanceComponent } from "./scan-performance.component";

describe("ScanTableComponent", () => {
  let component: ScanPerformanceComponent;
  let fixture: ComponentFixture<ScanPerformanceComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ScanPerformanceComponent]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScanPerformanceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
