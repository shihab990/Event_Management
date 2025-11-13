import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, EventDto, RegistrationRequest } from '../../core/api.service';

type RegisterForm = RegistrationRequest;

@Component({
  standalone: true,
  selector: 'app-events',
  imports: [CommonModule, FormsModule],
  template: `
  <div class="card">
    <h2>Available Events</h2>
    <div class="grid">
      @for (e of events; track e.id) {
        <div class="card">
          <h3>Event Name: {{e.name}}</h3>
          <div class="badge">{{e.location}}</div>
          <p>Description: {{e.description}}</p>
          <small>Time: {{e.startTime}} → {{e.endTime}}</small>

          <form class="grid mt register" (ngSubmit)="register(e.id)">
            <input placeholder="Your name" [(ngModel)]="forms[e.id].name" name="name-{{e.id}}" required>
            <input placeholder="Email" [(ngModel)]="forms[e.id].email" name="email-{{e.id}}" type="email" required>
            <input placeholder="Phone" [(ngModel)]="forms[e.id].phoneNumber" name="phoneNumber-{{e.id}}" required>
            <button>Register</button>
          </form>
        </div>
      }
    </div>
  </div>
  `
})
export class EventsComponent implements OnInit {
  api = inject(ApiService);
  events: EventDto[] = [];
  forms: Record<number, RegisterForm> = {};

  ngOnInit(){
    this.api.getEvents().subscribe(res => {
      for (const ev of res) {
        if (!this.forms[ev.id]) this.forms[ev.id] = { name:'', email:'', phoneNumber:'' };
      }
      this.events = res;
    });
  }

  register(eventId: number){
    const target = this.forms[eventId];
    if (!target) return;
    this.api.register(eventId, target).subscribe(() => {
      alert('Registered!');
      this.forms[eventId] = { name:'', email:'', phoneNumber:'' };
    });
  }
}
