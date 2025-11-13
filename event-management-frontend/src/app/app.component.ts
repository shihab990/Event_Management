import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  template: `
    <div class="container">
      <nav>
        <a routerLink="/">Events</a>
        @if (!auth.isLoggedIn()) {
          <a routerLink="/login">Login</a>
        }
        @if (auth.isLoggedIn()) {
          <a routerLink="/admin">Admin</a>
        }
        @if (auth.isLoggedIn()) {
          <button (click)="logout()">Logout</button>
        }
      </nav>
      <router-outlet></router-outlet>
    </div>
  `
})
export class AppComponent {
  auth = inject(AuthService);
  logout(){ this.auth.logout(); }
}
