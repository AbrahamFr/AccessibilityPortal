import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { PageFailureComponent } from "./page-failure.component";

describe("HeaderComponent", () => {
  let component: PageFailureComponent;
  let fixture: ComponentFixture<PageFailureComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [PageFailureComponent]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PageFailureComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
