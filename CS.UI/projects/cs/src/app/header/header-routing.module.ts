		
import { NgModule } from '@angular/core';
import { AppRoutingModule } from "../../routing/app-routing.module";
import { RouterModule, Routes } from '@angular/router';
import { HeaderComponent } from './header.component';

const headerRoutes: Routes = [
  {
    path: '',
    component: HeaderComponent,
  }
];

@NgModule({
  imports: [
    AppRoutingModule,
    RouterModule.forChild(headerRoutes)
  ],
  exports: [
    RouterModule
  ]
})
export class HeaderRoutingModule { }
