import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

declare const ngDevMode: boolean;

const API = getApiBaseUrl();

function isDevBuild(): boolean {
  return typeof ngDevMode !== 'undefined' && !!ngDevMode;
}

function isBrowser(): boolean {
  return typeof window !== 'undefined';
}

function getApiBaseUrl(): string {
  if (isDevBuild()) {
    return 'https://localhost:5001/api';
  }

  if (isBrowser()) {
    const host = window.location.hostname;
    if (!host || host === 'localhost' || host === '127.0.0.1') {
      return 'https://localhost:5001/api';
    }

    const manualOverride = (window as any).__API_BASE_URL__ as string | undefined;
    if (manualOverride) {
      return manualOverride;
    }

    return '/api';
  }

  return 'https://localhost:5001/api';
}

export interface EventDto {
  id: number;
  name: string;
  description: string;
  location: string;
  startTime: string;
  endTime: string;
}

export interface RegistrationDto {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  eventId: number;
}

export interface CreateEventPayload {
  name: string;
  description: string;
  location: string;
  startTime: string;
  endTime: string;
}

export interface RegistrationRequest {
  name: string;
  email: string;
  phoneNumber: string;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  login(username: string, password: string){
    return this.http.post<{token:string}>(`${API}/Auth/login`, { username, password });
  }

  getEvents(){
    return this.http.get<EventDto[]>(`${API}/Events`);
  }

  createEvent(payload: CreateEventPayload){
    return this.http.post<EventDto>(`${API}/Events/create`, payload);
  }

  deleteEvent(eventId: number){
    return this.http.delete<void>(`${API}/Events/${eventId}`);
  }

  register(eventId: number, body: RegistrationRequest){
    return this.http.post(`${API}/Events/${eventId}/register`, body);
  }

  getRegistrations(eventId: number){
    return this.http.get<RegistrationDto[]>(`${API}/Events/${eventId}/registrations`);
  }
}
