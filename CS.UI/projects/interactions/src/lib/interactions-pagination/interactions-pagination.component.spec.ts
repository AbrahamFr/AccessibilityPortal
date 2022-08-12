import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InteractionsPaginationComponent } from './interactions-pagination.component';

describe('InteractionsPaginationComponent', () => {
  let component: InteractionsPaginationComponent;
  let fixture: ComponentFixture<InteractionsPaginationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InteractionsPaginationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InteractionsPaginationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
