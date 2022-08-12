import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs';

@Component({
  selector: 'cinv-my-home',
  templateUrl: './my-home.component.html',
  styleUrls: ['./my-home.component.scss']
})
export class MyHomeComponent implements OnInit {

  pageFormView = "Start"
  editAuditData: { pageView: string, rowData: object };
  public selectedAudit?: object
  updateData: Subject<boolean> = new Subject<boolean>();
  
  constructor() { }

  ngOnInit(): void {
  }

  changeView(data: any)
  {
    this.editAuditData = data;
    this.selectedAudit = this.editAuditData.rowData;
    this.pageFormView = this.editAuditData.pageView;     
  }

  updateRecentAudits()
  {
    this.updateData.next(true)
  }
}
