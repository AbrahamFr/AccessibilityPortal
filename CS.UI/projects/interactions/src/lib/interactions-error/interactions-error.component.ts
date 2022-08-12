import { Component, OnInit, Input } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { ActivatedRoute } from "@angular/router";
// import { AppSettings } from "../../../../cs/src/app/constants/app-settings";

@Component({
  selector: "interactions-error",
  templateUrl: "./interactions-error.component.html",
  styleUrls: ["./interactions-error.component.scss"],
})
export class InteractionsErrorComponent implements OnInit {
  @Input()
  pageTitle: string;

  data: any;

  constructor(private route: ActivatedRoute, private titleService: Title) {
    const currentPageTitle = this.pageTitle
      ? this.pageTitle
      : "Compliance Sheriff";
    this.titleService.setTitle(currentPageTitle + " (Error)");
  }

  ngOnInit() {
    this.data = this.route.snapshot.data;
  }
}
