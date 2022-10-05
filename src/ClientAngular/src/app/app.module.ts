import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { ApiAuthorizationModule } from 'src/api-authorization/api-authorization.module';
import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';
import { UserGeneratedPasswordsPageComponent } from './user-generated-passwords-page/user-generated-passwords-page.component';
import { UserGeneratedPasswordService } from './services/UserGeneratedPasswordService';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    UserGeneratedPasswordsPageComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ApiAuthorizationModule,
    RouterModule.forRoot([
      { path: '', redirectTo: 'generated-passwords', pathMatch: 'full' },
      { path: 'generated-passwords', component: UserGeneratedPasswordsPageComponent, canActivate: [AuthorizeGuard] },
    ])
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
    UserGeneratedPasswordService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
