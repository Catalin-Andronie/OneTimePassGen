import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserGeneratedPasswordService } from '../services/UserGeneratedPasswordService';
import { UserGeneratedPasswordModel } from '../models/UserGeneratedPasswordModel';
import { Subscription } from 'rxjs/internal/Subscription';

@Component({
  selector: 'app-user-generated-passwords-page',
  templateUrl: './user-generated-passwords-page.component.html'
})
export class UserGeneratedPasswordsPageComponent implements OnInit, OnDestroy {
  private _userGeneratedPasswords: UserGeneratedPasswordModel[] = [];
  private _fetchUserGeneratedPasswordsSubscription: Subscription | undefined;
  private _createUserGeneratedPasswordSubscription: Subscription | undefined;

  public get isLoading(): boolean {
    return !!this._fetchUserGeneratedPasswordsSubscription && !this._fetchUserGeneratedPasswordsSubscription.closed;
  }

  public get isCreatingNewPassword(): boolean {
    return !!this._createUserGeneratedPasswordSubscription && !this._createUserGeneratedPasswordSubscription.closed;
  }

  public get userGeneratedPasswords(): UserGeneratedPasswordModel[] {
    if (this.displayExpiredUserGeneratedPasswords)
      return this._userGeneratedPasswords;
    else
      return this._userGeneratedPasswords.filter(p => !p.isExpired);
  };

  public displayExpiredUserGeneratedPasswords: boolean = true;

  public constructor(
    private _userGeneratedPasswordService: UserGeneratedPasswordService) {
  }

  public ngOnInit(): void {
    this._fetchUserGeneratedPasswords();
  }

  public ngOnDestroy(): void {
    this._fetchUserGeneratedPasswordsSubscription?.unsubscribe()
    this._createUserGeneratedPasswordSubscription?.unsubscribe()
  }

  public generateNewPassword(): void {
    this._createUserGeneratedPassword();
  }

  private _fetchUserGeneratedPasswords(): void {
    this._fetchUserGeneratedPasswordsSubscription?.unsubscribe()

    this._fetchUserGeneratedPasswordsSubscription = this._userGeneratedPasswordService.fetchUserGeneratedPasswords()
      .subscribe({
        next: (result) => this._userGeneratedPasswords = result || [],
        error: (e) => console.error(e),
      });
  }

  private _createUserGeneratedPassword(): void {
    this._createUserGeneratedPasswordSubscription?.unsubscribe()

    this._createUserGeneratedPasswordSubscription = this._userGeneratedPasswordService.createUserGeneratedPassword()
      .subscribe({
        next: (result) => this._userGeneratedPasswords = [result].concat(this._userGeneratedPasswords),
        error: (e) => console.error(e),
      });
  }
}
