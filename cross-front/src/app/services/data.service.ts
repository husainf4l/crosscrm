import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

// Define your data models here
export interface User {
  id: string;
  name: string;
  email: string;
}

export interface Lead {
  id: string;
  name: string;
  email: string;
  status: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {

  constructor(private api: ApiService) {}

  // GraphQL queries
  getUsers(): Observable<User[]> {
    const query = `
      query GetUsers {
        users {
          id
          name
          email
        }
      }
    `;

    return this.api.graphql<{ users: User[] }>(query).pipe(
      map(response => response.data?.users || [])
    );
  }

  getLeads(): Observable<Lead[]> {
    const query = `
      query GetLeads {
        leads {
          id
          name
          email
          status
          createdAt
        }
      }
    `;

    return this.api.graphql<{ leads: Lead[] }>(query).pipe(
      map(response => response.data?.leads || [])
    );
  }

  createLead(leadData: Partial<Lead>): Observable<Lead> {
    const mutation = `
      mutation CreateLead($input: CreateLeadInput!) {
        createLead(input: $input) {
          id
          name
          email
          status
          createdAt
        }
      }
    `;

    return this.api.graphql<{ createLead: Lead }>(mutation, { input: leadData }).pipe(
      map(response => response.data!.createLead)
    );
  }

  // Example of checking backend connection
  testConnection(): Observable<boolean> {
    const query = `
      query {
        __typename
      }
    `;

    return this.api.graphql(query).pipe(
      map(response => !!response.data && !response.errors)
    );
  }
}