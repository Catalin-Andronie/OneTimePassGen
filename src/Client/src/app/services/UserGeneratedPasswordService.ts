import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { map } from 'rxjs';
import { UserGeneratedPasswordModel } from "../models/UserGeneratedPasswordModel";
import { UserGeneratedPasswordDto } from '../models/UserGeneratedPasswordDto';
import { UserGeneratedPasswordMapper } from '../models/UserGeneratedPasswordMapper';

@Injectable()
export class UserGeneratedPasswordService {
  public constructor(
    private _httpClient: HttpClient,
    @Inject('BASE_URL') private _baseUrl: string) {
  }

  public fetchUserGeneratedPasswords(): Observable<UserGeneratedPasswordModel[]> {
    const url = this._baseUrl + 'api/user-generated-passwords';
    return this._httpClient.get<UserGeneratedPasswordModel[]>(url)
      .pipe(
        map((dtos: UserGeneratedPasswordDto[]) => {
          return UserGeneratedPasswordMapper.mapUserGeneratedPasswordModelFromDTOs(dtos);
        })
      );
  }

  public createUserGeneratedPassword(): Observable<UserGeneratedPasswordModel> {
    const url = this._baseUrl + 'api/user-generated-passwords';
    const payload = {};
    return this._httpClient.post<UserGeneratedPasswordModel>(url, payload)
      .pipe(
        map((dto: UserGeneratedPasswordDto) => {
          return UserGeneratedPasswordMapper.mapUserGeneratedPasswordModelFromDTO(dto);
        })
      );
  }
}
