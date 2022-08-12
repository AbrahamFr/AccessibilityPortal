import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TrendComponent } from './trend.component';

const trendRoutes: Routes = [
  {
    path: '',
    component: TrendComponent,
  }
];

@NgModule({
  imports: [
    RouterModule.forChild(trendRoutes)
  ],
  exports: [
    RouterModule
  ]
})
export class TrendRoutingModule { }
