import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-user-generated-passwords-page',
  templateUrl: './user-generated-passwords-page.component.html'
})
export class UserGeneratedPasswordsPageComponent {
  public userGeneratedPasswords: UserGeneratedPassword[] = [];

  public constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<UserGeneratedPassword[]>(baseUrl + 'api/user-generated-passwords')
      .subscribe({
        next: (result) => this.userGeneratedPasswords = result,
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
