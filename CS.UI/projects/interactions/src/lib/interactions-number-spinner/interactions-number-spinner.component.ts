import { Component, EventEmitter, forwardRef, Input, OnInit, Output } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'interactions-number-spinner',
  templateUrl: './interactions-number-spinner.component.html',
  styleUrls: ['./interactions-number-spinner.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InteractionsNumberSpinnerComponent),
      multi: true
    }
  ]  
})
export class InteractionsNumberSpinnerComponent implements ControlValueAccessor {

  @Input('value') value: number = 0;
  @Input('step') step: number = 1;
  @Input('min') min: number;
  @Input('max') max: number;
  @Input('direction') direction: 'horizontal'|'vertical' = 'horizontal';

  @Output('change') change: EventEmitter<number> = new EventEmitter();

  disabled: boolean;

  _propagateChange:any = () => {};
  _onTouched: () => void;  

  constructor() { }

  writeValue(value:number):void {
    this.value = value || 0;
  }

  registerOnChange(fn:any):void {
    this._propagateChange = fn;
  }

  registerOnTouched(fn:any):void {
    this._onTouched = fn;
  }

  setDisabledState?(isDisabled:boolean):void {
    this.disabled = isDisabled;
  }

  inc():void {
    this.value += this.step;
    this._onChange();
  }

  dec():void {
    this.value -= this.step;

    this._onChange();
  }

  private _onChange():void {
    this._propagateChange(this.value);

    this.change.emit(this.value);
  }

}
