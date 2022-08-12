import { TestBed, async } from "@angular/core/testing";
import { CinvComponent } from "./cinv.component";

describe("CinvComponent", () => {
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [CinvComponent],
    }).compileComponents();
  }));

  it("should create the app", () => {
    const fixture = TestBed.createComponent(CinvComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have as title 'compliance-investigate'`, () => {
    const fixture = TestBed.createComponent(CinvComponent);
    const app = fixture.componentInstance;
    expect(app.title).toEqual("compliance-investigate");
  });

  it("should render title", () => {
    const fixture = TestBed.createComponent(CinvComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement;
    expect(compiled.querySelector(".content span").textContent).toContain(
      "compliance-investigate app is running!"
    );
  });
});
