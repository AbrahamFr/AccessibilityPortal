import { ControlValueAccessor, NG_VALUE_ACCESSOR } from "@angular/forms";
import { Input, HostListener, ElementRef, Component } from "@angular/core";

@Component({
  selector: "app-file-upload",
  templateUrl: "./file-upload.component.html",
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: FileUploadComponent,
      multi: true
    }
  ]
})
export class FileUploadComponent implements ControlValueAccessor {
  @Input() progress;
  @Input() isDisabled: boolean = false;

  onChange: Function;
  file: File | null = null;

  @HostListener("change", ["$event.target.files"]) emitFiles(event: FileList) {
    const file = event && event.item(0);
    this.onChange(file);
    this.file = file;
  }

  constructor(private inputRef: ElementRef<HTMLInputElement>) {}

  // Both onChange and onTouched are functions
  onTouched = () => {};

  get value() {
    return this.file;
  }

  set value(file) {
    this.file = file;
    this.onChange(file);
    this.onTouched();
  }
  writeValue(value: null) {
    // clear file input
    this.inputRef.nativeElement.value = "";
    this.file = null;
  }

  registerOnChange(fn: (file: File) => void) {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void) {
    this.onTouched = fn;
  }
}
