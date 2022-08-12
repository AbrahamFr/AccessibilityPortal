import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { LoginService } from "authentication";

@Component({
  selector: "cinv-logout",
  templateUrl: "./logout.component.html",
  styleUrls: ["./logout.component.scss"],
})
export class LogoutComponent implements OnInit {
  constructor(private loginService: LoginService, private router: Router) {}

  ngOnInit(): void {
    this.loginService.logout();
    //Navigate to Login page
    this.router.navigate(["./login"]);
  }
}
