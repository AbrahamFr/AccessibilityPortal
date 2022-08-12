import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: "filterOptions" })
export class FilterOptionsPipe implements PipeTransform {
  // This removes the 'selectedOption' from the list of options in a Select element dropdown menu
  // Order of passed parameters is important!
  // Usage example: let group of scanGroups | filterOptions: selectedGroup:'scanGroupId':scanGroups
  transform(options: any[], selectedOption: any, prop: string) {
    if (options && selectedOption && prop) {
      return options.filter((item) => {
        return item[prop] !== selectedOption[prop];
      });
    }
  }
}
