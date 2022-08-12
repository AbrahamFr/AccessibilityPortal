import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GuidelinesService {

  constructor() { }

    calculateGuidelines(selection: string) : string[]
    {
        let guidelines : string[] = [];

        switch(selection)
        {
            case "W21_AA": {
              guidelines.push("W21_A", "W21_AA");
              break;
            }
              
            case "W21_AAA": {
              guidelines.push("W21_A", "W21_AA", "W21_AAA");
              break;
            }
                
            default: {
              guidelines.push(selection);
              break;
            }            
        }

        return guidelines;
    }
}
