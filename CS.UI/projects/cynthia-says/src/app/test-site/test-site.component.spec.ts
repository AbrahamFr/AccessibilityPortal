import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TestSiteComponent } from './test-site.component';

describe('TestSiteComponent', () => {
  let component: TestSiteComponent;
  let fixture: ComponentFixture<TestSiteComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TestSiteComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TestSiteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
