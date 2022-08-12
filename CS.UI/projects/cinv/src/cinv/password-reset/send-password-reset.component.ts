import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'cinv-send-password-reset',
  templateUrl: './send-password-reset.component.html',
  styleUrls: ['./send-password-reset.component.scss']
})
export class SendPasswordResetComponent implements OnInit {

  mode: string = "email-submission"

  constructor(private route: ActivatedRoute) { }

  ngOnInit(): void {    
    this.route.queryParams
    .subscribe(params => {
        if (params)
        {
          if (params.emailAddress)
          {
            this.mode = "reset-password"
          }
        }
    })
  }

}
