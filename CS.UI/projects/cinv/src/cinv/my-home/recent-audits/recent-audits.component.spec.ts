import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RecentAuditsComponent } from './recent-audits.component';

describe('RecentAuditsComponent', () => {
  let component: RecentAuditsComponent;
  let fixture: ComponentFixture<RecentAuditsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RecentAuditsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RecentAuditsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
