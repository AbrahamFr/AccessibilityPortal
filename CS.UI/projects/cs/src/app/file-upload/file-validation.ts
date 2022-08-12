import { FormControl, ValidatorFn } from "@angular/forms";

export function validateFileSize(maxFileSize: number): ValidatorFn {
  return function(control: FormControl) {
    const file = control.value;
    if (file) {
      if (file.size && file.size / (1024 * 1024) > maxFileSize) {
        return {
          validateFileSize: true
        };
      }
      return null;
    }
    return null;
  };
}

export function requiredFileType(fileTypesList: string): ValidatorFn {
  return function(control: FormControl) {
    const supportedFileTypes = fileTypesList.toLowerCase().split(",");
    const file = control.value;
    if (file) {
      const extension = file.name.split(".")[file.name.split(".").length - 1];
      if (!supportedFileTypes.includes(extension.toLowerCase())) {
        return {
          requiredFileType: true
        };
      }
      return null;
    }
    return null;
  };
}
