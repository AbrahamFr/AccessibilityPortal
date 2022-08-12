import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ApiHandlerComponent } from "./api-handler.component";
import { ApiHandlerErrorGuardComponent } from "./api-handler-error-guard/api-handler-error-guard.component";

@NgModule({
  declarations: [ApiHandlerComponent, ApiHandlerErrorGuardComponent],
  imports: [CommonModule],
  exports: [ApiHandlerComponent, ApiHandlerErrorGuardComponent],
})
export class ApiHandlerModule {}
