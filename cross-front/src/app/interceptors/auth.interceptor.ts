import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap, finalize } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(private authService: AuthService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Add auth token to request
    const authRequest = this.addAuthToken(request);

    return next.handle(authRequest).pipe(
      catchError((error: HttpErrorResponse) => {
        // Handle 401 errors - attempt token refresh
        if (error.status === 401 && this.authService.getToken()) {
          return this.handle401Error(authRequest, next);
        }
        
        return throwError(() => error);
      })
    );
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      // Attempt to refresh the token
      return (this.authService as any).refreshAccessToken().pipe(
        switchMap((token: any) => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(token.token);
          
          // Retry the original request with new token
          return next.handle(this.addAuthToken(request));
        }),
        catchError((refreshError) => {
          this.isRefreshing = false;
          
          // Refresh failed, sign out user
          this.authService.signOut();
          
          return throwError(() => refreshError);
        }),
        finalize(() => {
          this.isRefreshing = false;
        })
      );
    } else {
      // If already refreshing, wait for it to complete
      return this.refreshTokenSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap(() => next.handle(this.addAuthToken(request)))
      );
    }
  }

  private addAuthToken(request: HttpRequest<any>): HttpRequest<any> {
    const token = this.authService.getToken();
    
    if (token) {
      return request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    
    return request;
  }
}