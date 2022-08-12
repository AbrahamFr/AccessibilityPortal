import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { AuditReportUploadComponent } from './audit-report-upload.component';

describe('AuditReportUploadComponent', () => {
  let component: AuditReportUploadComponent;
  let fixture: ComponentFixture<AuditReportUploadComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AuditReportUploadComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditReportUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
