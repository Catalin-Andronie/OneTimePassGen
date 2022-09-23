import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-user-generated-passwords-page',
  templateUrl: './user-generated-passwords-page.component.html'
})
export class UserGeneratedPasswordsPageComponent implements OnInit {
  public userGeneratedPasswords: UserGeneratedPassword[] = [];

  public constructor(
    private _http: HttpClient,
    @Inject('BASE_URL') private _baseUrl: string)
  {
  }

  public ngOnInit(): void {
    this._fetchUserGeneratedPasswords();
  }

  private _fetchUserGeneratedPasswords(): void {
    const url = this._baseUrl + 'api/user-generated-passwords';
    this._http.get<UserGeneratedPassword[]>(url)
      .subscribe({
        next: (result) => this.userGeneratedPasswords = result || [],
        error: (e) => console.error(e)
      });
  }
}

interface UserGeneratedPassword {
  id: string;
  userId: string;
  password: string;
  expiersAt: Date;
  createdAt: Date;
}
