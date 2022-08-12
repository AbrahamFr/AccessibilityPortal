import { Location } from "@angular/common"
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class RoutePathService {
  location: Location;
  
  constructor(location: Location) { this.location = location }

  checkRouteForPath(routePath: string): boolean {
    const url = this.location.path();
    const pathSegments = url.toLowerCase().split("/");
    return pathSegments.includes(routePath.toLowerCase());
  }
}
