import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-input-validation-error',
  templateUrl: './input-validation-error.component.html',
  styleUrls: ['./input-validation-error.component.scss']
})
export class InputValidationErrorComponent implements OnInit {
  @Input()
  errorMessage: string;
  @Input()
  param?: any;
  @Input()
  styles?: any | null = {};

  constructor() { }

  ngOnInit() { 
  }

}
