import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { TranslateService } from "@ngx-translate/core";

@Component({
  selector: "app-test-site",
  templateUrl: "./test-site.component.html",
  styleUrls: ["./test-site.component.scss"]
})
export class TestSiteComponent implements OnInit {
  constructor(
    private fb: FormBuilder,
    private readonly translate: TranslateService) {}

  ngOnInit(): void {}

  siteAuditForm = this.fb.group({
    siteUrl: ["", Validators.required],
    complianceType: ["", Validators.required],
    email: ["", Validators.required],
    firstName: [""],
    lastName: [""],
    organization: [""]
  });
}
