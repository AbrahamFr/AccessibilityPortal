import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuditComponent } from './audit.component';

const auditsRoutes: Routes = [
  {
    path: '',
    component: AuditComponent,
  }
];

@NgModule({
  imports: [
    RouterModule.forChild(auditsRoutes)
  ],
  exports: [
    RouterModule
  ]
})
export class AuditsRoutingModule { }
