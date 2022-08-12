import { NgModule } from '@angular/core';
import { NavigationComponent } from './navigation.component';
import { UrlPathnamePipe } from './url-pathname.pipe';



@NgModule({
  declarations: [NavigationComponent, UrlPathnamePipe],
  imports: [
  ],
  exports: [NavigationComponent, UrlPathnamePipe]
})
export class NavigationModule { }
