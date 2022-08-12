import { NgModule } from '@angular/core';

import { ReportsComponent } from './reports.component';
import { ReportsRoutingModule } from './reports-routing.module';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    ReportsComponent,
  ],
  imports: [
    SharedModule,
    ReportsRoutingModule,
  ]
})
export class ReportsModule { }
