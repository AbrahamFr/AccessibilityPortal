import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { map } from 'rxjs/operators';
import { AuthenticationService } from './auth.service';
import { JwtStoreService } from './jwt-store.service';

@Injectable({
  providedIn: "root",
})
export class RefreshTokenService {
  private refreshTokenUrl: string = `rest/Authentication/refreshToken`;

  constructor(
    private http: HttpClient,
    private jwtStore: JwtStoreService,
    private authService: AuthenticationService
  
  ) {}

  refreshToken() {
    return this.http.get(this.refreshTokenUrl).pipe(
      map((response) => {
        this.authService.setToken(response["data"]["authToken"]);
        this.jwtStore.receiveJwtToken(null);
      })
    );
  }
}
