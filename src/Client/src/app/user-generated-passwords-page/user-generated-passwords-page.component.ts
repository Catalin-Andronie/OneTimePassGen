import { Component, OnInit } from '@angular/core';
import { UserGeneratedPasswordService } from '../services/UserGeneratedPasswordService';

@Component({
  selector: 'app-user-generated-passwords-page',
  templateUrl: './user-generated-passwords-page.component.html'
})
export class UserGeneratedPasswordsPageComponent implements OnInit {
  private _isLoading: boolean = false;
  private _isCreatingNewPassword: boolean = false;
  private _userGeneratedPasswords: UserGeneratedPassword[] = [];

  public get isLoading(): boolean { return this._isLoading; }
  public get isCreatingNewPassword(): boolean { return this._isCreatingNewPassword; }
  public get userGeneratedPasswords(): UserGeneratedPassword[] { return this._userGeneratedPasswords; };

  public constructor(
    private _userGeneratedPasswordService: UserGeneratedPasswordService) {
  }

  public ngOnInit(): void {
    this._fetchUserGeneratedPasswords();
  }

  public generateNewPassword(): void {
    this._createUserGeneratedPassword();
  }

  private _fetchUserGeneratedPasswords(): void {
    this._isLoading = true;
    this._userGeneratedPasswordService.fetchUserGeneratedPasswords()
      .subscribe({
        next: (result) => this._userGeneratedPasswords = result || [],
        error: (e) => console.error(e),
        complete: () => this._isLoading = false
      });
  }

  private _createUserGeneratedPassword(): void {
    this._isCreatingNewPassword = true;
    this._userGeneratedPasswordService.createUserGeneratedPassword()
      .subscribe({
        next: (result) => this._userGeneratedPasswords = [result].concat(this._userGeneratedPasswords),
        error: (e) => console.error(e),
        complete: () => this._isCreatingNewPassword = false
      });
  }
}

export interface UserGeneratedPassword {
  id: string;
  userId: string;
  password: string;
  expiersAt: Date;
  createdAt: Date;
}
