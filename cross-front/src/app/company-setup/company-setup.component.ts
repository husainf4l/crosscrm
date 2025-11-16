import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NavbarComponent } from '../components/layouts/navbar/navbar.component';
import { AuthService } from '../services/auth.service';

interface CompanySetupData {
  name: string;
  industry: string;
  size: string;
  website: string;
  countryCode: string;
  phone: string;
  address: string;
  city: string;
  country: string;
}

interface Country {
  code: string;
  name: string;
  dialCode: string;
  flag: string;
}

@Component({
  selector: 'app-company-setup',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    
    <div class="min-h-screen bg-gray-50 py-12">
      <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
        <!-- Header -->
        <div class="text-center mb-8">
          <h1 class="text-3xl font-light text-gray-900 mb-4">Complete Your Setup</h1>
          <p class="text-lg text-gray-600 max-w-2xl mx-auto">
            Let's set up your company profile to get the most out of your CRM experience.
          </p>
        </div>

        <!-- Progress Steps -->
        <div class="mb-8">
          <nav aria-label="Progress">
            <ol class="flex items-center justify-center space-x-8">
              <li class="flex items-center">
                <div class="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center">
                  <svg class="w-6 h-6 text-white" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                  </svg>
                </div>
                <span class="ml-3 text-sm font-medium text-gray-900">Account Created</span>
              </li>
              
              <li class="flex items-center">
                <div class="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center">
                  <span class="text-white font-medium">2</span>
                </div>
                <span class="ml-3 text-sm font-medium text-blue-600">Company Setup</span>
              </li>
              
              <li class="flex items-center">
                <div class="w-10 h-10 bg-gray-200 rounded-full flex items-center justify-center">
                  <span class="text-gray-600 font-medium">3</span>
                </div>
                <span class="ml-3 text-sm font-medium text-gray-500">Complete</span>
              </li>
            </ol>
          </nav>
        </div>

        <!-- Form -->
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
          <form (ngSubmit)="onSubmit()" #companyForm="ngForm">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Company Name -->
              <div class="md:col-span-2">
                <label for="name" class="block text-sm font-medium text-gray-700 mb-2">
                  Company Name *
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  required
                  [(ngModel)]="companyData.name"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Enter your company name">
              </div>

              <!-- Industry -->
              <div>
                <label for="industry" class="block text-sm font-medium text-gray-700 mb-2">
                  Industry *
                </label>
                <select
                  id="industry"
                  name="industry"
                  required
                  [(ngModel)]="companyData.industry"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500">
                  <option value="">Select an industry</option>
                  <option value="technology">Technology</option>
                  <option value="finance">Finance</option>
                  <option value="healthcare">Healthcare</option>
                  <option value="education">Education</option>
                  <option value="retail">Retail</option>
                  <option value="manufacturing">Manufacturing</option>
                  <option value="consulting">Consulting</option>
                  <option value="real-estate">Real Estate</option>
                  <option value="other">Other</option>
                </select>
              </div>

              <!-- Company Size -->
              <div>
                <label for="size" class="block text-sm font-medium text-gray-700 mb-2">
                  Company Size *
                </label>
                <select
                  id="size"
                  name="size"
                  required
                  [(ngModel)]="companyData.size"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500">
                  <option value="">Select company size</option>
                  <option value="1-10">1-10 employees</option>
                  <option value="11-50">11-50 employees</option>
                  <option value="51-200">51-200 employees</option>
                  <option value="201-500">201-500 employees</option>
                  <option value="500+">500+ employees</option>
                </select>
              </div>

              <!-- Website -->
              <div>
                <label for="website" class="block text-sm font-medium text-gray-700 mb-2">
                  Website
                </label>
                <input
                  type="url"
                  id="website"
                  name="website"
                  [(ngModel)]="companyData.website"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  placeholder="https://www.example.com">
              </div>

              <!-- Phone -->
              <div>
                <label for="phone" class="block text-sm font-medium text-gray-700 mb-2">
                  Phone Number
                </label>
                <div class="flex">
                  <select
                    [(ngModel)]="companyData.countryCode"
                    name="countryCode"
                    class="w-24 px-2 py-2 border border-gray-300 rounded-l-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 bg-gray-50">
                    <option *ngFor="let country of countries" [value]="country.dialCode">
                      {{ country.flag }} {{ country.dialCode }}
                    </option>
                  </select>
                  <input
                    type="tel"
                    id="phone"
                    name="phone"
                    [(ngModel)]="companyData.phone"
                    class="flex-1 px-3 py-2 border border-gray-300 rounded-r-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500 border-l-0"
                    placeholder="50 123 4567">
                </div>
                <p class="text-xs text-gray-500 mt-1">Enter phone number without country code</p>
              </div>

              <!-- Address -->
              <div class="md:col-span-2">
                <label for="address" class="block text-sm font-medium text-gray-700 mb-2">
                  Address
                </label>
                <input
                  type="text"
                  id="address"
                  name="address"
                  [(ngModel)]="companyData.address"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Street address">
              </div>

              <!-- City -->
              <div>
                <label for="city" class="block text-sm font-medium text-gray-700 mb-2">
                  City
                </label>
                <input
                  type="text"
                  id="city"
                  name="city"
                  [(ngModel)]="companyData.city"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                  placeholder="City">
              </div>

              <!-- Country -->
              <div>
                <label for="country" class="block text-sm font-medium text-gray-700 mb-2">
                  Country
                </label>
                <select
                  id="country"
                  name="country"
                  [(ngModel)]="companyData.country"
                  (ngModelChange)="onCountryChange()"
                  class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500">
                  <option value="">Select a country</option>
                  <option *ngFor="let country of countries" [value]="country.code">
                    {{ country.flag }} {{ country.name }}
                  </option>
                </select>
              </div>
            </div>

            <!-- Error Message -->
            <div *ngIf="errorMessage" class="mt-6 rounded-md bg-red-50 p-4">
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

            <!-- Form Actions -->
            <div class="mt-8 flex justify-between">
              <button
                type="button"
                (click)="skipSetup()"
                class="px-6 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
                Skip for Now
              </button>
              
              <button
                type="submit"
                [disabled]="isLoading || !companyForm.valid"
                class="px-6 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed">
                <svg *ngIf="isLoading" class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                {{ isLoading ? 'Creating Company...' : 'Complete Setup' }}
              </button>
            </div>
          </form>
        </div>

        <!-- Benefits Section -->
        <div class="mt-8 bg-blue-50 rounded-lg p-6">
          <h3 class="text-lg font-medium text-blue-900 mb-4">What happens next?</h3>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="flex items-start">
              <svg class="w-6 h-6 text-blue-600 mr-3 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
              <div>
                <h4 class="text-sm font-medium text-blue-900">Personalized Dashboard</h4>
                <p class="text-sm text-blue-800">Get insights tailored to your industry and company size.</p>
              </div>
            </div>
            
            <div class="flex items-start">
              <svg class="w-6 h-6 text-blue-600 mr-3 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.196-2.121M17 20v-2a3 3 0 00-5.196-2.121M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.196-2.121M7 20v2M7 20v-2m0 0a3 3 0 015.196-2.121M13 7a4 4 0 11-8 0 4 4 0 018 0z"></path>
              </svg>
              <div>
                <h4 class="text-sm font-medium text-blue-900">Team Collaboration</h4>
                <p class="text-sm text-blue-800">Invite team members and start collaborating immediately.</p>
              </div>
            </div>
            
            <div class="flex items-start">
              <svg class="w-6 h-6 text-blue-600 mr-3 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"></path>
              </svg>
              <div>
                <h4 class="text-sm font-medium text-blue-900">Quick Start</h4>
                <p class="text-sm text-blue-800">Access pre-configured templates and workflows.</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class CompanySetupComponent {
  companyData: CompanySetupData = {
    name: '',
    industry: '',
    size: '',
    website: '',
    countryCode: '+971', // Default to UAE
    phone: '',
    address: '',
    city: '',
    country: 'AE'
  };
  
  // Middle East countries with their dial codes
  countries: Country[] = [
    { code: 'AE', name: 'United Arab Emirates', dialCode: '+971', flag: '🇦🇪' },
    { code: 'SA', name: 'Saudi Arabia', dialCode: '+966', flag: '🇸🇦' },
    { code: 'QA', name: 'Qatar', dialCode: '+974', flag: '🇶🇦' },
    { code: 'KW', name: 'Kuwait', dialCode: '+965', flag: '🇰🇼' },
    { code: 'BH', name: 'Bahrain', dialCode: '+973', flag: '🇧🇭' },
    { code: 'OM', name: 'Oman', dialCode: '+968', flag: '🇴🇲' },
    { code: 'JO', name: 'Jordan', dialCode: '+962', flag: '🇯🇴' },
    { code: 'LB', name: 'Lebanon', dialCode: '+961', flag: '🇱🇧' },
    { code: 'SY', name: 'Syria', dialCode: '+963', flag: '🇸🇾' },
    { code: 'IQ', name: 'Iraq', dialCode: '+964', flag: '🇮🇶' },
    { code: 'IR', name: 'Iran', dialCode: '+98', flag: '🇮🇷' },
    { code: 'TR', name: 'Turkey', dialCode: '+90', flag: '🇹🇷' },
    { code: 'EG', name: 'Egypt', dialCode: '+20', flag: '🇪🇬' },
    { code: 'MA', name: 'Morocco', dialCode: '+212', flag: '🇲🇦' },
    { code: 'DZ', name: 'Algeria', dialCode: '+213', flag: '🇩🇿' },
    { code: 'TN', name: 'Tunisia', dialCode: '+216', flag: '🇹🇳' },
    { code: 'LY', name: 'Libya', dialCode: '+218', flag: '🇱🇾' },
    { code: 'SD', name: 'Sudan', dialCode: '+249', flag: '🇸🇩' },
    { code: 'YE', name: 'Yemen', dialCode: '+967', flag: '🇾🇪' },
    { code: 'AF', name: 'Afghanistan', dialCode: '+93', flag: '🇦🇫' },
    { code: 'PK', name: 'Pakistan', dialCode: '+92', flag: '🇵🇰' }
  ];
  
  isLoading = false;
  errorMessage = '';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  onCountryChange() {
    const selectedCountry = this.countries.find(c => c.code === this.companyData.country);
    if (selectedCountry) {
      this.companyData.countryCode = selectedCountry.dialCode;
    }
  }

  onSubmit() {
    if (!this.companyData.name || !this.companyData.industry || !this.companyData.size) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // Prepare company data for backend - only send the name for now as that's what the backend expects
    const companyDataToSend = {
      name: this.companyData.name
    };

    // Create company and set it up for the user
    this.authService.completeCompanySetup(companyDataToSend).subscribe({
      next: (company) => {
        console.log('Company created successfully:', company);
        this.isLoading = false;
        // Navigate to dashboard after successful company creation
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        console.error('Company creation failed:', error);
        this.errorMessage = error.message || 'Failed to create company. Please try again.';
        this.isLoading = false;
      }
    });
  }

  skipSetup() {
    // Allow users to skip the setup and go to dashboard with limited functionality
    this.router.navigate(['/dashboard']);
  }
}