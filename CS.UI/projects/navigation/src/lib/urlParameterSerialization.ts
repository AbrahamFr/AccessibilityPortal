import { ParamMap } from "@angular/router";
import { UrlParams, ImpactRange } from "cs-core";

export const filterTransform = (filter, value) => {
  if (filter) {
    return value ? value : undefined;
  }

  if (filter == "impactRange") {
    return value.map((v) => impactRangeTranslateToString(v));
  }

  return value;
};

export const impactRangeTranslateToString = (range: ImpactRange): string => {
  if (range.minImpact == 80 && range.maxImpact == 100) return "High";
  if (range.minImpact == 41 && range.maxImpact == 79) return "Med";
  return "Low";
};

export const impactRangeTranslateToRange = (type: string): ImpactRange => {
  if (type == "High") return { minImpact: 80, maxImpact: 100 };
  if (type == "Med") return { minImpact: 41, maxImpact: 79 };
  return { minImpact: 0, maxImpact: 40 };
};

export const searchParamsToUrlParams = (params: UrlParams): UrlParams => {
  return JSON.parse(JSON.stringify(params, filterTransform));
};

export const isParamMapObjEmpty = (obj: ParamMap): boolean => {
  return Object.keys(obj["params"]).length === 0;
};
