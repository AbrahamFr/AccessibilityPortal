import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: "filterAuditReportOptions" })
export class FilterAuditReportOptionsPipe implements PipeTransform {
  transform(options: string[], selectedReportType: string) {
    if (options && selectedReportType) {
      return options.filter(r => r !== selectedReportType);
    }
  }
}
