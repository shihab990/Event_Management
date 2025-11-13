import { Routes } from '@angular/router';
import { EventsComponent } from './features/events/events.component';
import { LoginComponent } from './features/login/login.component';
import { AdminComponent } from './features/admin/admin.component';

export const routes: Routes = [
  { path: '', component: EventsComponent },
  { path: 'login', component: LoginComponent },
  { path: 'admin', component: AdminComponent },
  { path: '**', redirectTo: '' }
];
