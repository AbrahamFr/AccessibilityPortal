import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: "humanize" })
export class HumanizePipe implements PipeTransform {
  // This Pipe transforms a camel case string into a space ' ' separated string, i.e., "IssueTracker" => "Issue Tracker"
  // For example, we pull the child route off of the url to determine which is the active breadcrumb.  That route segment is a camel case string ("IssueTracker"), but
  // for readability we display the string in a more readable fashion by separating the two words with a space.  This pipe handles that transform.  If not, the string passes thru unchanged
  transform(value: string) {
    if (typeof value !== "string") {
      return value;
    }
    value = value.split(/(?=[A-Z])/).join(" ");
    if(value && value[0]){
      value = value[0].toUpperCase() + value.slice(1);
    }
    return value;
  }
}
