import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { InteractionsErrorComponent } from "./interactions-error.component";

describe("ErrorComponent", () => {
  let component: InteractionsErrorComponent;
  let fixture: ComponentFixture<InteractionsErrorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [InteractionsErrorComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InteractionsErrorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
