import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, throwError, of } from 'rxjs';
import { map, catchError, tap, switchMap, filter, take, finalize } from 'rxjs/operators';
import { ApiService } from './api.service';

export interface User {
  id: string;
  email: string;
  name: string;
  avatar?: string;
  companyId?: string;
  companyName?: string;
  hasCompany?: boolean;
  company?: {
    id: string;
    name: string;
  };
}

export interface AuthResponse {
  user: User;
  token: string;
  refreshToken?: string;
  expiresIn?: number;
}

export interface SignInCredentials {
  email: string;
  password: string;
}

export interface SignUpData {
  name: string;
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private tokenSubject = new BehaviorSubject<string | null>(null);
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);
  private profileFetchInProgress = false;
  private refreshTokenTimeout: any;
  
  public currentUser$ = this.currentUserSubject.asObservable();
  public isAuthenticated$ = this.currentUser$.pipe(map(user => !!user));

  constructor(private apiService: ApiService) {
    // Check for existing token on initialization
    this.initializeAuth();
  }

  private initializeAuth(): void {
    const token = localStorage.getItem('access_token');
    const refreshToken = localStorage.getItem('refresh_token');
    const user = localStorage.getItem('current_user');
    
    if (token && user) {
      try {
        const parsedUser = JSON.parse(user);
        this.tokenSubject.next(token);
        this.refreshTokenSubject.next(refreshToken);
        this.currentUserSubject.next(parsedUser);
        
        // Check if token is expired and attempt refresh if needed
        if (this.isTokenExpired(token) && refreshToken) {
          this.refreshAccessToken().subscribe({
            next: () => {
              // Token refreshed successfully, proceed
              this.refreshUserProfile();
            },
            error: () => {
              // Refresh failed, clear auth
              this.clearAuth();
            }
          });
        } else {
          // Start automatic token refresh timer
          this.startTokenRefreshTimer(token);
          
          // Automatically refresh user profile to get latest data including avatar
          setTimeout(() => {
            this.refreshUserProfile();
          }, 100); // Small delay to let the app initialize
        }
        
      } catch (error) {
        this.clearAuth();
      }
    }
  }

  signIn(credentials: SignInCredentials): Observable<AuthResponse> {
    const mutation = `
      mutation Login($input: LoginDtoInput!) {
        login(input: $input) {
          token
          refreshToken
          expiresIn
          user {
            id
            email
            name
            avatar
            companyId
            companyName
          }
        }
      }
    `;

    return this.apiService.graphql<{ login: AuthResponse }>(mutation, { input: credentials })
      .pipe(
        map(response => {
          if (response.errors) {
            throw new Error(response.errors[0]?.message || 'Sign in failed');
          }
          const authResponse = response.data!.login;
          
          // Set hasCompany based on whether companyId exists
          const user: User = {
            ...authResponse.user,
            hasCompany: !!authResponse.user.companyId,
            company: authResponse.user.companyId ? {
              id: authResponse.user.companyId,
              name: authResponse.user.companyName || 'Company'
            } : undefined
          };

          return {
            ...authResponse,
            user
          };
        }),
        tap(authResponse => {
          this.setAuth(authResponse);
        }),
        catchError(error => {
          console.error('Sign in error:', error);
          return throwError(() => new Error(error.message || 'Sign in failed'));
        })
      );
  }

  signUp(userData: SignUpData): Observable<AuthResponse> {
    const mutation = `
      mutation Register($input: RegisterInputDto!) {
        register(input: $input) {
          token
          refreshToken
          expiresIn
          user {
            id
            email
            name
            avatar
            companyId
            companyName
          }
        }
      }
    `;

    return this.apiService.graphql<{ register: AuthResponse }>(mutation, { input: userData })
      .pipe(
        map(response => {
          if (response.errors) {
            throw new Error(response.errors[0]?.message || 'Sign up failed');
          }
          const authResponse = response.data!.register;
          
          // Set hasCompany based on whether companyId exists
          const user: User = {
            ...authResponse.user,
            hasCompany: !!authResponse.user.companyId,
            company: authResponse.user.companyId ? {
              id: authResponse.user.companyId,
              name: authResponse.user.companyName || 'Company'
            } : undefined
          };

          return {
            ...authResponse,
            user
          };
        }),
        tap(authResponse => {
          this.setAuth(authResponse);
        }),
        catchError(error => {
          console.error('Sign up error:', error);
          return throwError(() => new Error(error.message || 'Sign up failed'));
        })
      );
  }

  // Fetch current user profile from backend
  fetchCurrentUserProfile(): Observable<User> {
    // Prevent multiple simultaneous calls
    if (this.profileFetchInProgress) {
      return this.currentUser$.pipe(
        filter(user => !!user),
        take(1)
      ) as Observable<User>;
    }

    this.profileFetchInProgress = true;
    
    const query = `
      query Me {
        me {
          id
          email
          name
          avatar
          companyId
          companyName
        }
      }
    `;

    return this.apiService.graphql<{ me: User }>(query)
      .pipe(
        map(response => {
          if (response.errors) {
            throw new Error(response.errors[0]?.message || 'Failed to fetch user profile');
          }
          const userData = response.data!.me;
          
          // Create updated user object with company info
          const updatedUser: User = {
            ...userData,
            hasCompany: !!userData.companyId,
            company: userData.companyId ? {
              id: userData.companyId,
              name: userData.companyName || 'Company'
            } : undefined
          };
          
          // Update local storage and subject
          localStorage.setItem('current_user', JSON.stringify(updatedUser));
          this.currentUserSubject.next(updatedUser);
          
          return updatedUser;
        }),
        catchError(error => {
          console.error('Failed to fetch user profile:', error);
          // If it's an unauthorized error, clear auth
          if (error.message?.toLowerCase().includes('token') || 
              error.message?.toLowerCase().includes('unauthorized')) {
            this.clearAuth();
          }
          return throwError(() => new Error(error.message || 'Failed to fetch user profile'));
        }),
        finalize(() => {
          this.profileFetchInProgress = false;
        })
      );
  }

  // Refresh user profile automatically
  refreshUserProfile(): void {
    if (this.isAuthenticated()) {
      this.fetchCurrentUserProfile().subscribe({
        next: (user) => {
          console.log('User profile refreshed:', user);
        },
        error: (error) => {
          console.error('Failed to refresh user profile:', error);
          // If refresh fails due to invalid token, logout
          if (error.message?.includes('unauthorized') || error.message?.includes('token')) {
            this.signOut();
          }
        }
      });
    }
  }

  signOut(): void {
    // Call backend logout endpoint to invalidate the refresh token
    const refreshToken = this.getRefreshToken();
    
    if (refreshToken) {
      const mutation = `
        mutation Logout($refreshToken: String!) {
          logout(refreshToken: $refreshToken)
        }
      `;

      this.apiService.graphql<{ logout: boolean }>(mutation, { refreshToken })
        .subscribe({
          next: () => {
            console.log('Successfully logged out from backend');
          },
          error: (error) => {
            console.error('Backend logout error (continuing with local logout):', error);
          },
          complete: () => {
            this.clearAuth();
          }
        });
    } else {
      // No refresh token, just clear local auth
      this.clearAuth();
    }
  }

  // Refresh access token using refresh token
  refreshAccessToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    const mutation = `
      mutation RefreshToken($refreshToken: String!) {
        refreshToken(refreshToken: $refreshToken) {
          token
          refreshToken
          expiresIn
          user {
            id
            email
            name
            avatar
            companyId
            companyName
          }
        }
      }
    `;

    return this.apiService.graphql<{ refreshToken: AuthResponse }>(mutation, { refreshToken })
      .pipe(
        map(response => {
          if (response.errors) {
            throw new Error(response.errors[0]?.message || 'Token refresh failed');
          }
          const authResponse = response.data!.refreshToken;
          
          // Set hasCompany based on whether companyId exists
          const user: User = {
            ...authResponse.user,
            hasCompany: !!authResponse.user.companyId,
            company: authResponse.user.companyId ? {
              id: authResponse.user.companyId,
              name: authResponse.user.companyName || 'Company'
            } : undefined
          };

          return {
            ...authResponse,
            user
          };
        }),
        tap(authResponse => {
          this.setAuth(authResponse);
          this.startTokenRefreshTimer(authResponse.token);
        }),
        catchError(error => {
          console.error('Token refresh error:', error);
          this.clearAuth();
          return throwError(() => new Error(error.message || 'Token refresh failed'));
        })
      );
  }

  // Check if JWT token is expired
  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp < currentTime;
    } catch (error) {
      console.error('Error parsing token:', error);
      return true; // Assume expired if we can't parse
    }
  }

  // Start automatic token refresh timer
  private startTokenRefreshTimer(token: string): void {
    // Clear existing timer
    if (this.refreshTokenTimeout) {
      clearTimeout(this.refreshTokenTimeout);
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiresAt = payload.exp * 1000; // Convert to milliseconds
      const currentTime = Date.now();
      const timeUntilExpiry = expiresAt - currentTime;
      
      // Refresh token 5 minutes before it expires
      const refreshTime = timeUntilExpiry - (5 * 60 * 1000);
      
      if (refreshTime > 0) {
        this.refreshTokenTimeout = setTimeout(() => {
          console.log('Auto-refreshing token...');
          this.refreshAccessToken().subscribe({
            error: () => {
              console.log('Auto-refresh failed, user will need to login again');
            }
          });
        }, refreshTime);
      }
    } catch (error) {
      console.error('Error setting up token refresh timer:', error);
    }
  }

  private setAuth(authResponse: AuthResponse): void {
    // Store token and user data
    localStorage.setItem('access_token', authResponse.token);
    localStorage.setItem('current_user', JSON.stringify(authResponse.user));
    
    // Store refresh token if provided
    if (authResponse.refreshToken) {
      localStorage.setItem('refresh_token', authResponse.refreshToken);
      this.refreshTokenSubject.next(authResponse.refreshToken);
    }

    // Update subjects
    this.tokenSubject.next(authResponse.token);
    this.currentUserSubject.next(authResponse.user);
    
    // Start token refresh timer
    this.startTokenRefreshTimer(authResponse.token);
  }

  private clearAuth(): void {
    // Clear timeout
    if (this.refreshTokenTimeout) {
      clearTimeout(this.refreshTokenTimeout);
      this.refreshTokenTimeout = null;
    }
    
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('current_user');
    
    this.tokenSubject.next(null);
    this.refreshTokenSubject.next(null);
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  getRefreshToken(): string | null {
    return this.refreshTokenSubject.value;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.getCurrentUser() && !!this.getToken();
  }

  // Company related methods
  createCompany(companyData: any): Observable<any> {
    const mutation = `
      mutation CreateCompany($input: CreateCompanyDtoInput!) {
        createCompany(input: $input) {
          id
          name
        }
      }
    `;

    return this.apiService.graphql<{ createCompany: any }>(mutation, { input: companyData })
      .pipe(
        map(response => {
          if (response.errors) {
            throw new Error(response.errors[0]?.message || 'Failed to create company');
          }
          return response.data!.createCompany;
        }),
        catchError(error => {
          console.error('Failed to create company:', error);
          return throwError(() => new Error(error.message || 'Failed to create company'));
        })
      );
  }

  addUserToCompany(userId: number, companyId: number): Observable<boolean> {
    const mutation = `
      mutation AddUserToCompany($userId: Int!, $companyId: Int!) {
        addUserToCompany(userId: $userId, companyId: $companyId)
      }
    `;

    return this.apiService.graphql<{ addUserToCompany: boolean }>(mutation, { userId, companyId })
      .pipe(
        map(response => {
          if (response.errors) {
            throw new Error(response.errors[0]?.message || 'Failed to add user to company');
          }
          return response.data!.addUserToCompany;
        }),
        catchError(error => {
          console.error('Failed to add user to company:', error);
          return throwError(() => new Error(error.message || 'Failed to add user to company'));
        })
      );
  }

  setActiveCompany(userId: number, companyId: number): Observable<boolean> {
    const mutation = `
      mutation SetActiveCompany($userId: Int!, $companyId: Int!) {
        setActiveCompany(userId: $userId, companyId: $companyId)
      }
    `;

    return this.apiService.graphql<{ setActiveCompany: boolean }>(mutation, { userId, companyId })
      .pipe(
        map(response => {
          if (response.errors) {
            throw new Error(response.errors[0]?.message || 'Failed to set active company');
          }
          return response.data!.setActiveCompany;
        }),
        catchError(error => {
          console.error('Failed to set active company:', error);
          return throwError(() => new Error(error.message || 'Failed to set active company'));
        })
      );
  }

  // Utility method to decode JWT token for debugging
  decodeToken(token?: string): any {
    const tokenToUse = token || this.getToken();
    if (!tokenToUse) {
      return null;
    }

    try {
      const payload = JSON.parse(atob(tokenToUse.split('.')[1]));
      return {
        ...payload,
        exp_date: new Date(payload.exp * 1000),
        iat_date: new Date(payload.iat * 1000),
        is_expired: this.isTokenExpired(tokenToUse)
      };
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  // Method to manually test a token
  testToken(token: string): void {
    console.log('Testing JWT Token:');
    console.log('Raw token:', token);
    
    const decoded = this.decodeToken(token);
    if (decoded) {
      console.log('Decoded token:', decoded);
      console.log('User ID:', decoded.sub);
      console.log('Email:', decoded.email);
      console.log('Name:', decoded.name);
      console.log('Company ID:', decoded.companyId || 'No company ID in token');
      console.log('Issued at:', decoded.iat_date);
      console.log('Expires at:', decoded.exp_date);
      console.log('Is expired:', decoded.is_expired);
      console.log('Issuer:', decoded.iss);
      console.log('Audience:', decoded.aud);
      
      // Test what the improved login would look like
      console.log('\n--- Simulating improved login response ---');
      console.log('This is what the response would look like with company data:');
      console.log({
        token: token,
        refreshToken: 'refresh_token_here',
        expiresIn: 3600,
        user: {
          id: decoded.sub,
          email: decoded.email,
          name: decoded.name,
          avatar: 'https://ui-avatars.com/api/?name=' + encodeURIComponent(decoded.name || 'User'),
          companyId: decoded.companyId || null,
          companyName: decoded.companyId ? 'Papaya Trading' : null,
          hasCompany: !!decoded.companyId
        }
      });
    }
  }

  // Complete company setup process
  completeCompanySetup(companyData: any): Observable<any> {
    const currentUser = this.getCurrentUser();
    if (!currentUser) {
      return throwError(() => new Error('User not authenticated'));
    }

    return this.createCompany(companyData).pipe(
      switchMap(company => {
        // Add user to the created company
        return this.addUserToCompany(parseInt(currentUser.id), company.id).pipe(
          map(() => company)
        );
      }),
      switchMap(company => {
        // Set the company as active for the user
        return this.setActiveCompany(parseInt(currentUser.id), company.id).pipe(
          map(() => company)
        );
      }),
      tap(() => {
        // Refresh user profile to get updated company info
        this.refreshUserProfile();
      })
    );
  }
}