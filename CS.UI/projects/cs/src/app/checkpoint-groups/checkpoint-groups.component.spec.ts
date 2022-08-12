import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckpointGroupsComponent } from './checkpoint-groups.component';

describe('CheckpointGroupsComponent', () => {
  let component: CheckpointGroupsComponent;
  let fixture: ComponentFixture<CheckpointGroupsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckpointGroupsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckpointGroupsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
