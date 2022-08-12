import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditReportDownloadComponent } from './audit-report-download.component';

describe('AuditReportDownloadComponent', () => {
  let component: AuditReportDownloadComponent;
  let fixture: ComponentFixture<AuditReportDownloadComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuditReportDownloadComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditReportDownloadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
