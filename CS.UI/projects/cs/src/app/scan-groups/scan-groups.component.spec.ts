import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ScanGroupsComponent } from './scan-groups.component';

describe('ScanGroupsComponent', () => {
  let component: ScanGroupsComponent;
  let fixture: ComponentFixture<ScanGroupsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ScanGroupsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScanGroupsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
