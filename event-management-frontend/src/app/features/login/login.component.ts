import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  template: `
  <div class="card">
    <h2>Creator Login</h2>
    <form (ngSubmit)="submit()">
      <div class="grid">
        <input placeholder="Username" [(ngModel)]="username" name="username" required>
        <input type="password" placeholder="Password" [(ngModel)]="password" name="password" required>
      </div>
      <button class="mt login">Login</button>
      @if (error) {
        <div class="badge login">Invalid credentials</div>
      }
    </form>
  </div>
  `
})
export class LoginComponent {
  api = inject(ApiService);
  auth = inject(AuthService);
  router = inject(Router);
  username = '';
  password = '';
  error = false;

  submit(){
    this.error = false;
    this.api.login(this.username, this.password).subscribe({
      next: res => { this.auth.setToken(res.token); this.router.navigateByUrl('/admin'); },
      error: _ => this.error = true
    });
  }
}
