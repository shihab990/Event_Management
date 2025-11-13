import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenSig = signal<string | null>(localStorage.getItem('jwt'));

  get token() { return this.tokenSig(); }
  isLoggedIn() { return !!this.tokenSig(); }

  setToken(t: string){
    localStorage.setItem('jwt', t);
    this.tokenSig.set(t);
  }
  logout(){
    localStorage.removeItem('jwt');
    this.tokenSig.set(null);
    location.href = '/';
  }
}
