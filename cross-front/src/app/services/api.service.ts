import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface GraphQLQuery {
  query: string;
  variables?: any;
}

export interface GraphQLResponse<T = any> {
  data?: T;
  errors?: any[];
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private graphqlEndpoint = environment.graphqlEndpoint;
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // GraphQL query method
  graphql<T = any>(query: string, variables?: any): Observable<GraphQLResponse<T>> {
    const body: GraphQLQuery = {
      query,
      variables
    };

    return this.http.post<GraphQLResponse<T>>(this.graphqlEndpoint, body, {
      headers: {
        'Content-Type': 'application/json'
      }
    });
  }

  // REST API methods
  get<T = any>(endpoint: string): Observable<T> {
    return this.http.get<T>(`${this.apiUrl}${endpoint}`);
  }

  post<T = any>(endpoint: string, data: any): Observable<T> {
    return this.http.post<T>(`${this.apiUrl}${endpoint}`, data);
  }

  put<T = any>(endpoint: string, data: any): Observable<T> {
    return this.http.put<T>(`${this.apiUrl}${endpoint}`, data);
  }

  delete<T = any>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.apiUrl}${endpoint}`);
  }

  // Utility method to check if API is available
  checkApiHealth(): Observable<any> {
    return this.http.get(`${this.apiUrl}/health`).pipe(
      // Add error handling if needed
    );
  }
}