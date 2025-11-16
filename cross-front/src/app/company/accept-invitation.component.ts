import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

import { CompanyService } from '../services/company.service';
import { AuthService } from '../services/auth.service';
import { ApiService } from '../services/api.service';

interface InvitationDetails {
  email: string;
  companyName: string;
  invitedByUserName: string;
  roleName?: string;
  teamName?: string;
  createdAt: string;
  expiresAt: string;
  status: string;
}

interface AcceptInvitationRequest {
  invitationToken: string;
  name: string;
  password: string;
  phone?: string;
}

interface AcceptInvitationResponse {
  success: boolean;
  message: string;
  invitation?: any;
  authResponse?: {
    token: string;
    user: any;
  };
}

@Component({
  selector: 'app-accept-invitation',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div class="sm:mx-auto sm:w-full sm:max-w-md">
        <div class="text-center">
          <h2 class="text-3xl font-light text-gray-900">Join Your Team</h2>
          <p class="mt-2 text-sm text-gray-600">Accept your invitation to get started</p>
        </div>
      </div>

      <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <!-- Loading State -->
        <div *ngIf="isLoading" class="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
          <div class="text-center">
            <svg class="w-8 h-8 animate-spin text-blue-600 mx-auto" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-13a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V5z" clip-rule="evenodd"></path>
            </svg>
            <p class="mt-2 text-sm text-gray-600">Loading invitation...</p>
          </div>
        </div>

        <!-- Invalid/Expired Invitation -->
        <div *ngIf="!isLoading && !invitationDetails" class="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
          <div class="text-center">
            <svg class="w-12 h-12 text-red-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.5 0L4.268 18.5c-.77.833.192 2.5 1.732 2.5z"></path>
            </svg>
            <h3 class="text-lg font-medium text-gray-900 mb-2">Invalid Invitation</h3>
            <p class="text-sm text-gray-600 mb-4">
              This invitation link is invalid, expired, or has already been used.
            </p>
            <button 
              (click)="goToSignIn()"
              class="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 transition-colors">
              Go to Sign In
            </button>
          </div>
        </div>

        <!-- Invitation Details & Accept Form -->
        <div *ngIf="!isLoading && invitationDetails" class="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
          <!-- Invitation Info -->
          <div class="mb-6 p-4 bg-blue-50 rounded-lg border border-blue-200">
            <div class="flex items-center">
              <svg class="w-5 h-5 text-blue-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
              </svg>
              <h3 class="text-sm font-medium text-blue-900">You've been invited to join</h3>
            </div>
            <div class="mt-2">
              <p class="text-lg font-semibold text-blue-900">{{ invitationDetails.companyName }}</p>
              <p class="text-sm text-blue-700">
                Invited by {{ invitationDetails.invitedByUserName }} to {{ invitationDetails.email }}
              </p>
              <div class="flex items-center space-x-4 mt-2" *ngIf="invitationDetails.roleName || invitationDetails.teamName">
                <span *ngIf="invitationDetails.roleName" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                  {{ invitationDetails.roleName }}
                </span>
                <span *ngIf="invitationDetails.teamName" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                  {{ invitationDetails.teamName }}
                </span>
              </div>
              <p class="text-xs text-blue-600 mt-2">
                Expires {{ formatDate(invitationDetails.expiresAt) }}
              </p>
            </div>
          </div>

          <!-- Accept Form -->
          <form [formGroup]="acceptForm" (ngSubmit)="acceptInvitation()">
            <!-- Full Name -->
            <div class="mb-4">
              <label for="name" class="block text-sm font-medium text-gray-700 mb-2">
                Full Name *
              </label>
              <input 
                id="name"
                type="text" 
                formControlName="name"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter your full name">
              <p *ngIf="acceptForm.get('name')?.errors?.['required'] && acceptForm.get('name')?.touched" 
                 class="mt-1 text-sm text-red-600">Full name is required</p>
              <p *ngIf="acceptForm.get('name')?.errors?.['minlength'] && acceptForm.get('name')?.touched" 
                 class="mt-1 text-sm text-red-600">Name must be at least 2 characters</p>
            </div>

            <!-- Password -->
            <div class="mb-4">
              <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
                Password *
              </label>
              <div class="relative">
                <input 
                  id="password"
                  [type]="showPassword ? 'text' : 'password'"
                  formControlName="password"
                  class="w-full px-3 py-2 pr-10 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Create a secure password">
                <button 
                  type="button"
                  (click)="showPassword = !showPassword"
                  class="absolute inset-y-0 right-0 pr-3 flex items-center">
                  <svg *ngIf="!showPassword" class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                  </svg>
                  <svg *ngIf="showPassword" class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21"></path>
                  </svg>
                </button>
              </div>
              <p *ngIf="acceptForm.get('password')?.errors?.['required'] && acceptForm.get('password')?.touched" 
                 class="mt-1 text-sm text-red-600">Password is required</p>
              <p *ngIf="acceptForm.get('password')?.errors?.['minlength'] && acceptForm.get('password')?.touched" 
                 class="mt-1 text-sm text-red-600">Password must be at least 6 characters</p>
            </div>

            <!-- Confirm Password -->
            <div class="mb-4">
              <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-2">
                Confirm Password *
              </label>
              <input 
                id="confirmPassword"
                type="password"
                formControlName="confirmPassword"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Confirm your password">
              <p *ngIf="acceptForm.get('confirmPassword')?.errors?.['required'] && acceptForm.get('confirmPassword')?.touched" 
                 class="mt-1 text-sm text-red-600">Please confirm your password</p>
              <p *ngIf="acceptForm.errors?.['passwordMismatch'] && acceptForm.get('confirmPassword')?.touched" 
                 class="mt-1 text-sm text-red-600">Passwords do not match</p>
            </div>

            <!-- Phone (Optional) -->
            <div class="mb-6">
              <label for="phone" class="block text-sm font-medium text-gray-700 mb-2">
                Phone Number
              </label>
              <input 
                id="phone"
                type="tel" 
                formControlName="phone"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="+1 (555) 123-4567">
            </div>

            <!-- Error Message -->
            <div *ngIf="errorMessage" class="mb-4 bg-red-50 border border-red-200 rounded-lg p-3">
              <p class="text-sm text-red-600">{{ errorMessage }}</p>
            </div>

            <!-- Submit Button -->
            <button 
              type="submit" 
              [disabled]="acceptForm.invalid || isAccepting"
              class="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center justify-center space-x-2">
              <svg *ngIf="isAccepting" class="w-5 h-5 animate-spin" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-13a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V5z" clip-rule="evenodd"></path>
              </svg>
              <span>{{ isAccepting ? 'Creating Account...' : 'Accept Invitation & Create Account' }}</span>
            </button>
          </form>

          <!-- Already have an account -->
          <div class="mt-6 text-center">
            <p class="text-sm text-gray-600">
              Already have an account? 
              <button 
                (click)="goToSignIn()" 
                class="text-blue-600 hover:text-blue-800 font-medium">
                Sign in instead
              </button>
            </p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class AcceptInvitationComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  invitationToken = '';
  invitationDetails: InvitationDetails | null = null;
  acceptForm: FormGroup;

  isLoading = true;
  isAccepting = false;
  showPassword = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private companyService: CompanyService,
    private authService: AuthService,
    private apiService: ApiService
  ) {
    this.acceptForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
      confirmPassword: ['', Validators.required],
      phone: ['', Validators.maxLength(20)]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit() {
    this.invitationToken = this.route.snapshot.paramMap.get('token') || '';
    
    if (!this.invitationToken) {
      this.router.navigate(['/signin']);
      return;
    }

    this.loadInvitationDetails();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    
    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true };
    }
    
    return null;
  }

  private loadInvitationDetails() {
    const query = `
      query GetInvitationByToken($token: String!) {
        invitationByToken(token: $token) {
          email
          companyName
          invitedByUserName
          roleName
          teamName
          createdAt
          expiresAt
          status
        }
      }
    `;

    this.apiService.graphql<{ invitationByToken: InvitationDetails }>(query, { token: this.invitationToken })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          
          if (response.errors) {
            console.error('GraphQL errors:', response.errors);
            this.invitationDetails = null;
            return;
          }

          const invitation = response.data?.invitationByToken;
          if (!invitation || invitation.status !== 'PENDING') {
            this.invitationDetails = null;
            return;
          }

          // Check if invitation is expired
          const expiresAt = new Date(invitation.expiresAt);
          if (expiresAt < new Date()) {
            this.invitationDetails = null;
            return;
          }

          this.invitationDetails = invitation;
        },
        error: (error) => {
          console.error('Error loading invitation:', error);
          this.isLoading = false;
          this.invitationDetails = null;
        }
      });
  }

  acceptInvitation() {
    if (this.acceptForm.invalid) return;

    this.isAccepting = true;
    this.errorMessage = '';

    const acceptData: AcceptInvitationRequest = {
      invitationToken: this.invitationToken,
      name: this.acceptForm.value.name,
      password: this.acceptForm.value.password,
      phone: this.acceptForm.value.phone || undefined
    };

    const mutation = `
      mutation AcceptInvitation($input: AcceptInvitationDtoInput!) {
        acceptInvitation(input: $input) {
          success
          message
          invitation {
            id
            email
            companyName
            status
            acceptedAt
          }
          authResponse {
            token
            user {
              id
              name
              email
              companyId
              companyName
            }
          }
        }
      }
    `;

    this.apiService.graphql<{ acceptInvitation: AcceptInvitationResponse }>(mutation, { input: acceptData })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isAccepting = false;

          if (response.errors) {
            this.errorMessage = response.errors[0]?.message || 'Failed to accept invitation';
            return;
          }

          const result = response.data!.acceptInvitation;
          
          if (!result.success) {
            this.errorMessage = result.message || 'Failed to accept invitation';
            return;
          }

          console.log('Invitation accepted successfully:', result);

          // Store authentication token
          if (result.authResponse?.token) {
            // Store the token
            localStorage.setItem('token', result.authResponse.token);
            console.log('Token stored successfully');
            
            // Force a small delay to ensure token is stored
            setTimeout(() => {
              // Redirect to dashboard - the auth service will automatically fetch user profile
              console.log('Redirecting to dashboard...');
              this.router.navigate(['/dashboard']);
            }, 100);
          } else {
            this.errorMessage = 'Authentication failed. Please try signing in.';
          }
        },
        error: (error) => {
          console.error('Error accepting invitation:', error);
          this.errorMessage = error.message || 'Failed to accept invitation';
          this.isAccepting = false;
        }
      });
  }

  goToSignIn() {
    this.router.navigate(['/signin']);
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = date.getTime() - now.getTime();
    const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24));

    if (diffDays === 0) {
      return 'today';
    } else if (diffDays === 1) {
      return 'tomorrow';
    } else if (diffDays > 1) {
      return `in ${diffDays} days`;
    } else {
      return 'expired';
    }
  }
}