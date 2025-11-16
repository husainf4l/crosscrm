import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NavbarComponent } from '../components/layouts/navbar/navbar.component';
import { AuthService, SignUpData } from '../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, RouterLink],
  template: `
    <app-navbar></app-navbar>
    
    <div class="pt-12 min-h-screen flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div class="sm:mx-auto sm:w-full sm:max-w-md">
        <h2 class="mt-6 text-center text-3xl font-light tracking-tight text-gray-900">
          Create your account
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          Already have an account?
          <a routerLink="/signin" class="font-medium text-blue-600 hover:text-blue-500">
            Sign in
          </a>
        </p>
      </div>

      <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <div class="bg-white py-8 px-4 shadow-sm rounded-lg sm:px-10 border border-gray-200">
          <form class="space-y-6" (ngSubmit)="onRegister()" #registerForm="ngForm">
            <!-- Name Field -->
            <div>
              <label for="name" class="block text-sm font-medium text-gray-700">
                Full name
              </label>
              <div class="mt-1">
                <input
                  id="name"
                  name="name"
                  type="text"
                  required
                  [(ngModel)]="userData.name"
                  class="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  placeholder="Enter your full name">
              </div>
            </div>

            <!-- Email Field -->
            <div>
              <label for="email" class="block text-sm font-medium text-gray-700">
                Email address
              </label>
              <div class="mt-1">
                <input
                  id="email"
                  name="email"
                  type="email"
                  autocomplete="email"
                  required
                  [(ngModel)]="userData.email"
                  class="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  placeholder="Enter your email">
              </div>
            </div>

            <!-- Password Field -->
            <div>
              <label for="password" class="block text-sm font-medium text-gray-700">
                Password
              </label>
              <div class="mt-1">
                <input
                  id="password"
                  name="password"
                  type="password"
                  required
                  [(ngModel)]="userData.password"
                  class="appearance-none block w-full px-3 py-2 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  placeholder="Create a password">
              </div>
            </div>

            <!-- Error Message -->
            <div *ngIf="errorMessage" class="rounded-md bg-red-50 p-4">
              <div class="flex">
                <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                </svg>
                <div class="ml-3">
                  <h3 class="text-sm font-medium text-red-800">
                    {{ errorMessage }}
                  </h3>
                </div>
              </div>
            </div>

            <!-- Submit Button -->
            <div>
              <button
                type="submit"
                [disabled]="isLoading || !registerForm.valid"
                class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed">
                <svg *ngIf="isLoading" class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                {{ isLoading ? 'Creating account...' : 'Get started' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {
  userData: SignUpData = {
    name: '',
    email: '',
    password: ''
  };
  
  isLoading = false;
  errorMessage = '';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  onRegister() {
    if (!this.userData.name || !this.userData.email || !this.userData.password) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.signUp(this.userData).subscribe({
      next: (response) => {
        console.log('Registration successful:', response);
        // Redirect to home since user is now signed in
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('Registration error:', error);
        this.errorMessage = error.message || 'Registration failed. Please try again.';
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}