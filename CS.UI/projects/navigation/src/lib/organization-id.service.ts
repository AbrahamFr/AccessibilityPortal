import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root",
})
export class OrganizationIdService {
  constructor() {}

  private organizationVirtualDir: string | null;
  private _useOrgVirtualDir: boolean = true;
  public get useOrgVirtualDir() {
    const localUseOrgVirtualDir = localStorage.getItem("useOrgVirtualDir");
    if (localUseOrgVirtualDir && localUseOrgVirtualDir === "false") {
      this._useOrgVirtualDir = false;
    }
    return this._useOrgVirtualDir;
  }

  public set useOrgVirtualDir(value: boolean) {
    localStorage.setItem("useOrgVirtualDir", value === true ? "true" : "false");
    this._useOrgVirtualDir = value;
  }

  public get orgVirtualDir() {
    return this.organizationVirtualDir;
  }
  public set orgVirtualDir(orgVirtualDir: string | null) {
    this.organizationVirtualDir = orgVirtualDir;
  }
}
