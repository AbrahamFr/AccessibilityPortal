import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditReportEditComponent } from './audit-report-edit.component';

describe('AuditReportEditComponent', () => {
  let component: AuditReportEditComponent;
  let fixture: ComponentFixture<AuditReportEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuditReportEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditReportEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
