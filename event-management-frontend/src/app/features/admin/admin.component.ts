import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, CreateEventPayload, EventDto, RegistrationDto } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';

type CreateEventForm = {
  name: string;
  description: string;
  location: string;
  startTimeLocal: string;
  endTimeLocal: string;
};

@Component({
  standalone: true,
  selector: 'app-admin',
  imports: [CommonModule, FormsModule],
  template: `
  @if (auth.isLoggedIn()) {
  <div class="card">
    <h2>Admin</h2>

    <h3>Create Event</h3>
    <form class="grid grid-2" (ngSubmit)="create()">
      <input placeholder="Name" [(ngModel)]="createForm.name" name="name" required>
      <input placeholder="Location" [(ngModel)]="createForm.location" name="location" required>
      <input type="datetime-local" [(ngModel)]="createForm.startTimeLocal" name="start" required>
      <input type="datetime-local" [(ngModel)]="createForm.endTimeLocal" name="end" required>
      <textarea placeholder="Description" [(ngModel)]="createForm.description" name="desc" class="col-span-2"></textarea>
      <button class="col-span-2">Create</button>
    </form>

    <h3 class="mt event">Your Events</h3>
    <div class="grid">
      @for (e of events; track e.id) {
        <div class="card">
          <h4>{{e.name}}</h4>
          <small>{{e.startTime}} → {{e.endTime}}</small>
          <div class="grid grid-2 mt-sm">
            <button (click)="loadRegs(e.id)">Load registrations</button>
            <button class="danger" (click)="deleteEvent(e.id)">Delete</button>
          </div>
          @if (regs[e.id]) {
            <div>
              <h5>Registrations ({{regs[e.id].length}})</h5>
              <ul>
              @for (r of regs[e.id]; track $index) {
                <li>
                  {{ r.name }} — {{ r.email }} — {{ r.phoneNumber }}
                </li>
              }
              </ul>
            </div>
          }
        </div>
      }
    </div>
  </div>
  } @else {
    <ng-template #loginInfo></ng-template>
  }
  <ng-template #loginInfo>
    <div class="card">Please login to access admin.</div>
  </ng-template>
  `
})
export class AdminComponent implements OnInit {
  api = inject(ApiService);
  auth = inject(AuthService);

  events: EventDto[] = [];
  regs: Record<number, RegistrationDto[]> = {};
  createForm: CreateEventForm = this.buildEmptyForm();

  ngOnInit(){
    this.refresh();
  }

  refresh(){
    this.api.getEvents().subscribe(res => this.events = res);
  }

  create(){
    const startTime = this.normalizeLocalDate(this.createForm.startTimeLocal);
    const endTime = this.normalizeLocalDate(this.createForm.endTimeLocal);
    if (!startTime || !endTime) {
      alert('Please select both start and end times.');
      return;
    }

    const payload = {
      name: this.createForm.name,
      description: this.createForm.description,
      location: this.createForm.location,
      startTime,
      endTime
    } satisfies CreateEventPayload;

    this.api.createEvent(payload).subscribe(ev => {
      this.refresh();
      this.createForm = this.buildEmptyForm();
    });
  }

  loadRegs(eventId: number){
    this.api.getRegistrations(eventId).subscribe(r => this.regs[eventId] = r);
  }

  deleteEvent(eventId: number){
    if (!confirm('Delete this event?')) return;
    this.api.deleteEvent(eventId).subscribe(() => {
      delete this.regs[eventId];
      this.refresh();
    });
  }

  private buildEmptyForm(): CreateEventForm {
    return { name: '', description: '', location: '', startTimeLocal: '', endTimeLocal: '' };
  }

  private normalizeLocalDate(value: string): string | null {
    if (!value) return null;
    // datetime-local returns 'YYYY-MM-DDTHH:mm' (without seconds/timezone).
    // Keep the original local time so the backend stores exactly what the user entered.
    if (/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(value)) {
      return `${value}:00`;
    }
    return value;
  }
}
