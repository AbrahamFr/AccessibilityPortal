import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UrlParserService {

  constructor() { }

  parseUrl(baseUrl: string, getBaseUrl: boolean = false)
  {    
    const url = new URL(baseUrl);

    if (getBaseUrl)
    {
      return url.origin
    }
    const urlpathName = url.pathname;

    if (urlpathName.length > 1)
    {
      return urlpathName.substring(1)
    }

    return urlpathName
  }  
}
