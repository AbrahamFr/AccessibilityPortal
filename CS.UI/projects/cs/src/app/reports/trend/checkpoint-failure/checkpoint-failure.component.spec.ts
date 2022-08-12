import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { CheckpointFailureComponent } from "./checkpoint-failure.component";

describe("HeaderComponent", () => {
  let component: CheckpointFailureComponent;
  let fixture: ComponentFixture<CheckpointFailureComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [CheckpointFailureComponent]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckpointFailureComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
