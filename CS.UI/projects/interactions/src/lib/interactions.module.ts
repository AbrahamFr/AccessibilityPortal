import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { TranslateModule, TranslateLoader } from "@ngx-translate/core";

import { InteractionsComponent } from "./interactions.component";
import { InteractionsErrorComponent } from "./interactions-error/interactions-error.component";
import { TypeaheadComponent } from "./typeahead/typeahead.component";
import { InputValidationErrorComponent } from "./input-validation-error/input-validation-error.component";
import { FilterOptionsPipe } from "./filter-options.pipe";
import { FocusDirective } from "./focus.directive";
import { OutsideClickDirective } from "./outside-click.directive";
import { SetFocusNextDirective } from "./set-focus-next.directive";
import { SetFocusDirective } from "./set-focus.directive";
import { ToggleListDirective } from "./toggle-list.directive";
import { InteractionsPaginationComponent } from './interactions-pagination/interactions-pagination.component';
import { InteractionsNumberSpinnerComponent } from './interactions-number-spinner/interactions-number-spinner.component';

export function createInteractionsTranslateLoader(http: HttpClient) {
  return new TranslateHttpLoader(http, "./lib/assets/i18n/", "-lang.json");
}
@NgModule({
  declarations: [
    InteractionsComponent,
    InteractionsErrorComponent,
    InputValidationErrorComponent,
    TypeaheadComponent,
    FilterOptionsPipe,
    FocusDirective,
    OutsideClickDirective,
    SetFocusNextDirective,
    SetFocusDirective,
    ToggleListDirective,
    InteractionsPaginationComponent,
    InteractionsNumberSpinnerComponent,
  ],
  imports: [
    CommonModule,
    TranslateModule.forChild({
      loader: {
        provide: TranslateLoader,
        useFactory: createInteractionsTranslateLoader,
        deps: [HttpClient],
      },
      isolate: true,
    }),
  ],
  exports: [
    InteractionsComponent,
    InteractionsErrorComponent,
    InputValidationErrorComponent,
    TypeaheadComponent,
    FilterOptionsPipe,
    FocusDirective,
    OutsideClickDirective,
    SetFocusNextDirective,
    SetFocusDirective,
    ToggleListDirective,
    InteractionsPaginationComponent,
    InteractionsNumberSpinnerComponent,
  ],
})
export class InteractionsModule {}
