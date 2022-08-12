import { Injectable } from "@angular/core";
import { ActivatedRoute, ActivatedRouteSnapshot } from "@angular/router";
import { OrganizationIdService } from "./organization-id.service";

@Injectable({
  providedIn: "root"
})
export class OrganizationResolver {
  constructor(private organizationId: OrganizationIdService) {}
  public getOrgIdFromRouteSnapshot(
    activatedRouteSnapshot: ActivatedRouteSnapshot
  ) {
    return this.updateOrgVirtualDir(
      activatedRouteSnapshot.paramMap.get("orgId")
    );
  }

  public getOrgIdFromRoute(activatedRoute: ActivatedRoute) {
    return this.updateOrgVirtualDir(activatedRoute.snapshot.params["orgId"]);
  }

  private updateOrgVirtualDir(orgVirtualDir: string | null) {
    if (
      Boolean(orgVirtualDir) &&
      orgVirtualDir !== this.organizationId.orgVirtualDir
    ) {
      this.organizationId.orgVirtualDir = orgVirtualDir;
    }
    return this.organizationId.orgVirtualDir;
  }
}
