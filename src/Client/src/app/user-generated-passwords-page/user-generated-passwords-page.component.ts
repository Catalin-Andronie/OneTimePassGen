import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-user-generated-passwords-page',
  templateUrl: './user-generated-passwords-page.component.html'
})
export class UserGeneratedPasswordsPageComponent implements OnInit {
  private _isLoading: boolean = false;

  public get isLoading(): boolean { return this._isLoading; }
  public userGeneratedPasswords: UserGeneratedPassword[] = [];

  public constructor(
    private _httpClient: HttpClient,
    @Inject('BASE_URL') private _baseUrl: string)
  {
  }

  public ngOnInit(): void {
    this._fetchUserGeneratedPasswords();
  }

  private _fetchUserGeneratedPasswords(): void {
    this._isLoading = true;
    const url = this._baseUrl + 'api/user-generated-passwords';
    this._httpClient.get<UserGeneratedPassword[]>(url)
      .subscribe({
        next: (result) => this.userGeneratedPasswords = result || [],
        error: (e) => console.error(e),
        complete: () => this._isLoading = false
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
