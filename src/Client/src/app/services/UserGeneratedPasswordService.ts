import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { UserGeneratedPassword } from "../models/UserGeneratedPassword";

@Injectable()
export class UserGeneratedPasswordService {
  public constructor(
    private _httpClient: HttpClient,
    @Inject('BASE_URL') private _baseUrl: string) {
  }

  public fetchUserGeneratedPasswords(): Observable<UserGeneratedPassword[]> {
    const url = this._baseUrl + 'api/user-generated-passwords';
    return this._httpClient.get<UserGeneratedPassword[]>(url);
  }

  public createUserGeneratedPassword(): Observable<UserGeneratedPassword> {
    const url = this._baseUrl + 'api/user-generated-passwords';
    const payload = {};
    return this._httpClient.post<UserGeneratedPassword>(url, payload);
  }
}
