import { Injectable } from "@angular/core";
import { KeyValueString, OccurrenceParams } from "cs-core";
import { UrlParamsService } from "navigation";
import { OccurrenceList } from "../../../types";
import { OccurrencesParamService } from "../../occurrences-params.service";
import { IssueTrackerParamService } from "../../../issue-tracker-param.service";

export type OccurrenceFilters = {
  pageTitle: KeyValueString | null | undefined;
  pageUrl: string | null | undefined;
  element: KeyValueString | null | undefined;
  keyAttribute: KeyValueString | null | undefined;
  containerId: KeyValueString | null | undefined;
};

@Injectable({
  providedIn: "root",
})
export class OccurrenceFilterService {
  allFilters: OccurrenceParams;

  occurrenceFilters: OccurrenceFilters;
  titleFilterList: KeyValueString[];
  urlFilterList: string[];
  elementFilterList: KeyValueString[];
  keyAttributeFilterList: KeyValueString[];
  containerIdFilterList: KeyValueString[];

  constructor(
    private issueTrackerParamService: IssueTrackerParamService,
    private urlParamService: UrlParamsService,
    private occurrencesParamService: OccurrencesParamService
  ) {
    this.initializeStateOccurrencesFilter();
  }

  initializeStateOccurrencesFilter() {
    this.occurrenceFilters = {
      pageTitle: null,
      pageUrl: null,
      element: null,
      keyAttribute: null,
      containerId: null,
    };
  }

  updateOccurrencesFiltersFromParams(
    occurrenceList: OccurrenceList
  ): OccurrenceFilters {
    this.setFilterLists(occurrenceList);
    const occurrenceParams = this.issueTrackerParamService.getOccurrences();
    this.occurrenceFilters = {
      pageTitle:
        occurrenceParams.pageTitle === null ||
        occurrenceParams.pageTitle === undefined
          ? null
          : this.titleFilterList.find(
              (t) => t.key === occurrenceParams.pageTitle
            ),
      pageUrl: !occurrenceParams.pageUrl ? null : occurrenceParams.pageUrl,
      element:
        occurrenceParams.element === null ||
        occurrenceParams.element === undefined
          ? null
          : this.elementFilterList.find(
              (e) => e.key === occurrenceParams.element
            ),
      keyAttribute:
        occurrenceParams.keyAttribute === null ||
        occurrenceParams.keyAttribute === undefined
          ? null
          : this.keyAttributeFilterList.find(
              (k) => k.key === occurrenceParams.keyAttribute
            ),
      containerId:
        occurrenceParams.containerId === null ||
        occurrenceParams.containerId === undefined
          ? null
          : this.containerIdFilterList.find(
              (c) => c.key === occurrenceParams.containerId
            ),
    };
    return this.occurrenceFilters;
  }

  private setFilterLists(occurrenceList: OccurrenceList) {
    this.titleFilterList = !occurrenceList.titleFilterList
      ? []
      : occurrenceList.titleFilterList;
    this.urlFilterList = !occurrenceList.urlFilterList
      ? []
      : occurrenceList.urlFilterList;
    this.elementFilterList = !occurrenceList.elementFilterList
      ? []
      : occurrenceList.elementFilterList;
    this.keyAttributeFilterList = !occurrenceList.keyAttributeFilterList
      ? []
      : occurrenceList.keyAttributeFilterList;
    this.containerIdFilterList = !occurrenceList.containerIdFilterList
      ? []
      : occurrenceList.containerIdFilterList;
  }

  updateFilterForPageTitle(pageTitleValue: string) {
    this.occurrenceFilters.pageTitle = this.titleFilterList.find(
      (t) => t.value === pageTitleValue
    );
  }
  updateFilterForPageUrl(pageUrl: string) {
    this.occurrenceFilters.pageUrl = this.urlFilterList.find(
      (u) => u === pageUrl
    );
  }
  updateFilterForHtmlElement(htmlElementValue: string) {
    this.occurrenceFilters.element = this.elementFilterList.find(
      (e) => e.value === htmlElementValue
    );
  }
  updateFilterForKeyAttribute(keyAttributeValue: string) {
    this.occurrenceFilters.keyAttribute = this.keyAttributeFilterList.find(
      (k) => k.value === keyAttributeValue
    );
  }
  updateFilterForContainerId(containerIdValue: string) {
    this.occurrenceFilters.containerId = this.containerIdFilterList.find(
      (c) => c.value === containerIdValue
    );
  }

  submitOccurrencesFilter(occurrenceSearchFilters: string[]) {
    const allFilters = {
      pageTitle: this.occurrenceFilters.pageTitle?.key,
      pageUrl: !this.occurrenceFilters.pageUrl
        ? undefined
        : this.occurrenceFilters.pageUrl,
      element: this.occurrenceFilters.element?.key,
      keyAttribute: this.occurrenceFilters.keyAttribute?.key,
      containerId: this.occurrenceFilters.containerId?.key,
      recordsToReturn: 20,
    };
    let summaryFilters = this.issueTrackerParamService.getOccurrencesSummaryFilters();
    occurrenceSearchFilters
      ? (summaryFilters = {
          ...summaryFilters,
          ...{ occurrenceSearchFilters: occurrenceSearchFilters },
        })
      : null;
    const params = {
      ...allFilters,
      ...{
        summaryFilters,
      },
    };
    const appParamsObj = this.occurrencesParamService.updateOccurrenceIssueTrackerAppParamsObj(
      params
    );
    const updatedUrlDataParams = this.issueTrackerParamService.updateIssueTrackerUrlDataParams(
      appParamsObj
    );
    this.urlParamService.updateUrlParams(updatedUrlDataParams);
  }
}
