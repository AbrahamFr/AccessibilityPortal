import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { HeaderComponent } from "./header.component";
import { NavMenuComponent } from "./nav-menu/nav-menu.component";
import { BrowserModule } from "@angular/platform-browser";
import { HeaderRoutingModule } from "./header-routing.module";
import { MenuDropdownComponent } from './nav-menu/menu-dropdown/menu-dropdown.component';

@NgModule({
  declarations: [HeaderComponent, NavMenuComponent, MenuDropdownComponent],
  imports: [BrowserModule, CommonModule, HeaderRoutingModule],
  exports: [HeaderComponent],
})
export class HeaderModule {}
