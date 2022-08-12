import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { OccurrencesExportComponent } from "./occurrences-export.component";

describe("OccurrencesExportComponent", () => {

  let component: OccurrencesExportComponent;
  let fixture: ComponentFixture<OccurrencesExportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OccurrencesExportComponent]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OccurrencesExportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
