import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditProcessingPageComponent } from './audit-processing-page.component';

describe('AuditProcessingPageComponent', () => {
  let component: AuditProcessingPageComponent;
  let fixture: ComponentFixture<AuditProcessingPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuditProcessingPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditProcessingPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
