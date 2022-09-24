import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { UserGeneratedPasswordModel } from "../models/UserGeneratedPasswordModel";

@Injectable()
export class UserGeneratedPasswordService {
  public constructor(
    private _httpClient: HttpClient,
    @Inject('BASE_URL') private _baseUrl: string) {
  }

  public fetchUserGeneratedPasswords(): Observable<UserGeneratedPasswordModel[]> {
    const url = this._baseUrl + 'api/user-generated-passwords';
    return this._httpClient.get<UserGeneratedPasswordModel[]>(url);
  }

  public createUserGeneratedPassword(): Observable<UserGeneratedPasswordModel> {
    const url = this._baseUrl + 'api/user-generated-passwords';
    const payload = {};
    return this._httpClient.post<UserGeneratedPasswordModel>(url, payload);
  }
}
