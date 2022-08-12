import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IssueTrackerComponent } from './issue-tracker.component';
import { OccurrencesComponent } from './occurrences/occurrences.component';

const issueTrackerRoutes: Routes = [
  {
    path: '',
    component: IssueTrackerComponent,
  },
  {
    path: "Occurrences",
    component: OccurrencesComponent
  },
];

@NgModule({
  imports: [
    RouterModule.forChild(issueTrackerRoutes)
  ],
  exports: [
    RouterModule
  ]
})
export class IssueTrackerRoutingModule { }
