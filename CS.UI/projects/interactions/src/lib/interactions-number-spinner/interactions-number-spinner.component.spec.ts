import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InteractionsNumberSpinnerComponent } from './interactions-number-spinner.component';

describe('InteractionsNumberSpinnerComponent', () => {
  let component: InteractionsNumberSpinnerComponent;
  let fixture: ComponentFixture<InteractionsNumberSpinnerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InteractionsNumberSpinnerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InteractionsNumberSpinnerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
