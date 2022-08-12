import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditReportDeleteComponent } from './audit-report-delete.component';

describe('AuditReportDeleteComponent', () => {
  let component: AuditReportDeleteComponent;
  let fixture: ComponentFixture<AuditReportDeleteComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuditReportDeleteComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditReportDeleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
