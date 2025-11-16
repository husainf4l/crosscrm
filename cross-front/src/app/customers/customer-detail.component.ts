import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomerService, Customer } from '../services/customer.service';

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="min-h-screen bg-gray-50/30 p-6">
      <div class="max-w-6xl mx-auto">
        <!-- Header -->
        <div class="mb-8">
          <div class="flex items-center justify-between">
            <div class="flex items-center">
              <button 
                (click)="goBack()"
                class="mr-4 p-2 text-gray-400 hover:text-gray-600 transition-colors duration-200">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"></path>
                </svg>
              </button>
              <div>
                <h1 class="text-3xl font-light tracking-tight text-gray-900">
                  {{ customer?.companyName || 'Customer Details' }}
                </h1>
                <p class="mt-2 text-gray-600">{{ customer?.industry }} • {{ customer?.customerType }}</p>
              </div>
            </div>
            <div class="flex items-center space-x-3">
              <button 
                *ngIf="!isEditing"
                (click)="toggleEdit()"
                class="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-xl text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors duration-200">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
                </svg>
                Edit
              </button>
              <button 
                *ngIf="isEditing"
                (click)="saveChanges()"
                [disabled]="loading || (editForm && editForm.invalid)"
                class="inline-flex items-center px-4 py-2 border border-transparent rounded-xl shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-200">
                <svg 
                  *ngIf="loading"
                  class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" 
                  fill="none" 
                  viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Save Changes
              </button>
              <button 
                *ngIf="isEditing"
                (click)="cancelEdit()"
                class="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-xl text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors duration-200">
                Cancel
              </button>
            </div>
          </div>
        </div>

        <!-- Loading State -->
        <div *ngIf="loading && !customer" class="flex justify-center items-center py-12">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>

        <!-- Error State -->
        <div *ngIf="error" class="bg-red-50 border border-red-200 rounded-xl p-6 mb-6">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
              </svg>
            </div>
            <div class="ml-3">
              <h3 class="text-sm font-medium text-red-800">Error</h3>
              <p class="mt-1 text-sm text-red-700">{{ error }}</p>
            </div>
          </div>
        </div>

        <!-- Customer Details -->
        <div *ngIf="customer" class="grid grid-cols-1 lg:grid-cols-3 gap-6">
          
          <!-- Main Information -->
          <div class="lg:col-span-2 space-y-6">
            
            <!-- Basic Information -->
            <div class="bg-white/80 backdrop-blur-sm rounded-xl shadow-sm border border-gray-200/50 p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-6">Basic Information</h3>
              
              <div *ngIf="!isEditing" class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <dt class="text-sm font-medium text-gray-500">Company Name</dt>
                  <dd class="mt-1 text-lg text-gray-900">{{ customer.companyName }}</dd>
                </div>
                <div>
                  <dt class="text-sm font-medium text-gray-500">Contact Person</dt>
                  <dd class="mt-1 text-lg text-gray-900">{{ customer.contactPersonName || '-' }}</dd>
                </div>
                <div>
                  <dt class="text-sm font-medium text-gray-500">Customer Type</dt>
                  <dd class="mt-1 text-lg text-gray-900">{{ customer.customerType }}</dd>
                </div>
                <div>
                  <dt class="text-sm font-medium text-gray-500">Industry</dt>
                  <dd class="mt-1 text-lg text-gray-900">{{ customer.industry || '-' }}</dd>
                </div>
                <div>
                  <dt class="text-sm font-medium text-gray-500">Website</dt>
                  <dd class="mt-1 text-lg text-gray-900">
                    <a *ngIf="customer.website" [href]="customer.website" target="_blank" class="text-blue-600 hover:text-blue-800">
                      {{ customer.website }}
                    </a>
                    <span *ngIf="!customer.website">-</span>
                  </dd>
                </div>
                <div>
                  <dt class="text-sm font-medium text-gray-500">Source</dt>
                  <dd class="mt-1 text-lg text-gray-900">{{ customer.source }}</dd>
                </div>
                <div *ngIf="customer.description" class="md:col-span-2">
                  <dt class="text-sm font-medium text-gray-500">Description</dt>
                  <dd class="mt-1 text-gray-900">{{ customer.description }}</dd>
                </div>
              </div>

              <!-- Edit Form -->
              <form *ngIf="isEditing && editForm" [formGroup]="editForm" class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="md:col-span-2">
                  <label class="block text-sm font-medium text-gray-700 mb-2">Company Name</label>
                  <input 
                    type="text"
                    formControlName="companyName"
                    class="block w-full px-3 py-2 border border-gray-300 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Contact Person</label>
                  <input 
                    type="text"
                    formControlName="contactPersonName"
                    class="block w-full px-3 py-2 border border-gray-300 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Customer Type</label>
                  <select 
                    formControlName="customerType"
                    class="block w-full px-3 py-2 border border-gray-300 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
                    <option value="lead">Lead (Initial Contact)</option>
                    <option value="prospect">Prospect (Qualified Lead)</option>
                    <option value="customer">Active Customer (Paying)</option>
                    <option value="client">Established Client (Regular Buyer)</option>
                    <option value="enterprise">Enterprise Client (Large Account)</option>
                    <option value="partner">Business Partner</option>
                    <option value="dormant">Dormant (Previous Customer)</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Industry</label>
                  <input 
                    type="text"
                    formControlName="industry"
                    class="block w-full px-3 py-2 border border-gray-300 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Website</label>
                  <input 
                    type="url"
                    formControlName="website"
                    class="block w-full px-3 py-2 border border-gray-300 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
                </div>
                <div class="md:col-span-2">
                  <label class="block text-sm font-medium text-gray-700 mb-2">Description</label>
                  <textarea 
                    formControlName="description"
                    rows="3"
                    class="block w-full px-3 py-2 border border-gray-300 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"></textarea>
                </div>
              </form>
            </div>

            <!-- Business Information -->
            <div class="bg-white/80 backdrop-blur-sm rounded-xl shadow-sm border border-gray-200/50 p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-6">Business Information</h3>
              
              <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div>
                  <dt class="text-sm font-medium text-gray-500">Annual Revenue</dt>
                  <dd class="mt-1 text-lg text-gray-900">
                    {{ customer.annualRevenue ? (customer.annualRevenue | currency:'USD':'symbol':'1.0-0') : '-' }}
                  </dd>
                </div>
                <div>
                  <dt class="text-sm font-medium text-gray-500">Employee Count</dt>
                  <dd class="mt-1 text-lg text-gray-900">{{ customer.employeeCount || '-' }}</dd>
                </div>
                <div>
                  <dt class="text-sm font-medium text-gray-500">Tax ID</dt>
                  <dd class="mt-1 text-lg text-gray-900">{{ customer.taxId || '-' }}</dd>
                </div>
              </div>
            </div>

            <!-- Contact Information -->
            <div class="bg-white/80 backdrop-blur-sm rounded-xl shadow-sm border border-gray-200/50 p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-6">Contact Information</h3>
              
              <div *ngIf="customer.contacts && customer.contacts.length > 0" class="space-y-4">
                <div 
                  *ngFor="let contact of customer.contacts"
                  class="p-4 bg-gray-50/50 rounded-xl border border-gray-200">
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-sm font-medium text-gray-900">
                      {{ contact.type | titlecase }}
                      <span *ngIf="contact.isPrimary" class="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                        Primary
                      </span>
                    </span>
                    <span *ngIf="contact.label" class="text-sm text-gray-500">{{ contact.label }}</span>
                  </div>
                  <p class="text-gray-900">{{ contact.value }}</p>
                </div>
              </div>
              
              <p *ngIf="!customer.contacts || customer.contacts.length === 0" class="text-gray-500 italic">
                No contact information available
              </p>
            </div>

            <!-- Address Information -->
            <div class="bg-white/80 backdrop-blur-sm rounded-xl shadow-sm border border-gray-200/50 p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-6">Address Information</h3>
              
              <div *ngIf="customer.addresses && customer.addresses.length > 0" class="space-y-4">
                <div 
                  *ngFor="let address of customer.addresses"
                  class="p-4 bg-gray-50/50 rounded-xl border border-gray-200">
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-sm font-medium text-gray-900">
                      {{ address.type | titlecase }}
                      <span *ngIf="address.isPrimary" class="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                        Primary
                      </span>
                    </span>
                    <span *ngIf="address.label" class="text-sm text-gray-500">{{ address.label }}</span>
                  </div>
                  <div class="text-gray-900">
                    <p>{{ address.addressLine1 }}</p>
                    <p *ngIf="address.addressLine2">{{ address.addressLine2 }}</p>
                    <p>{{ address.city }}, {{ address.state }} {{ address.postalCode }}</p>
                    <p>{{ address.country }}</p>
                  </div>
                </div>
              </div>
              
              <p *ngIf="!customer.addresses || customer.addresses.length === 0" class="text-gray-500 italic">
                No address information available
              </p>
            </div>
          </div>

          <!-- Sidebar Information -->
          <div class="space-y-6">
            
            <!-- Status Card -->
            <div class="bg-white/80 backdrop-blur-sm rounded-xl shadow-sm border border-gray-200/50 p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Status</h3>
              
              <div class="space-y-4">
                <div>
                  <dt class="text-sm font-medium text-gray-500">Status</dt>
                  <dd class="mt-1">
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                          [ngClass]="{
                            'bg-green-100 text-green-800': customer.status === 'active',
                            'bg-emerald-100 text-emerald-800': customer.status === 'purchasing',
                            'bg-gray-100 text-gray-800': customer.status === 'inactive',
                            'bg-orange-100 text-orange-800': customer.status === 'lost',
                            'bg-red-100 text-red-800': customer.status === 'blacklisted'
                          }">
                      {{ customer.status | titlecase }}
                    </span>
                  </dd>
                </div>
                
                <div>
                  <dt class="text-sm font-medium text-gray-500">Priority</dt>
                  <dd class="mt-1">
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                          [ngClass]="{
                            'bg-red-100 text-red-800': customer.priority === 'high' || customer.priority === 'critical',
                            'bg-yellow-100 text-yellow-800': customer.priority === 'medium',
                            'bg-green-100 text-green-800': customer.priority === 'low'
                          }">
                      {{ customer.priority | titlecase }}
                    </span>
                  </dd>
                </div>
              </div>
            </div>

            <!-- Timeline Card -->
            <div class="bg-white/80 backdrop-blur-sm rounded-xl shadow-sm border border-gray-200/50 p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Timeline</h3>
              
              <div class="space-y-4">
                <div>
                  <dt class="text-sm font-medium text-gray-500">Created</dt>
                  <dd class="mt-1 text-sm text-gray-900">{{ customer.createdAt | date:'medium' }}</dd>
                </div>
                
                <div>
                  <dt class="text-sm font-medium text-gray-500">Last Updated</dt>
                  <dd class="mt-1 text-sm text-gray-900">{{ customer.updatedAt | date:'medium' }}</dd>
                </div>
              </div>
            </div>

            <!-- Actions Card -->
            <div class="bg-white/80 backdrop-blur-sm rounded-xl shadow-sm border border-gray-200/50 p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">Actions</h3>
              
              <div class="space-y-3">
                <button class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded-xl transition-colors duration-200">
                  View Activities
                </button>
                <button class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded-xl transition-colors duration-200">
                  Create Opportunity
                </button>
                <button class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded-xl transition-colors duration-200">
                  Schedule Meeting
                </button>
                <button class="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded-xl transition-colors duration-200">
                  Send Email
                </button>
                <hr class="border-gray-200">
                <button class="w-full text-left px-3 py-2 text-sm text-red-600 hover:bg-red-50 rounded-xl transition-colors duration-200">
                  Delete Customer
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class CustomerDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private customerService = inject(CustomerService);
  private fb = inject(FormBuilder);

  customer: Customer | null = null;
  loading = false;
  error: string | null = null;
  isEditing = false;
  editForm: FormGroup | null = null;

  ngOnInit() {
    this.loadCustomer();
  }

  private loadCustomer() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = 'No customer ID provided';
      return;
    }

    this.loading = true;
    this.customerService.getCustomerById(id).subscribe({
      next: (customer: Customer) => {
        this.customer = customer;
        this.loading = false;
        this.initEditForm();
      },
      error: (error: any) => {
        console.error('Error loading customer:', error);
        this.error = 'Failed to load customer details';
        this.loading = false;
      }
    });
  }

  private initEditForm() {
    if (!this.customer) return;

    this.editForm = this.fb.group({
      companyName: [this.customer.companyName, Validators.required],
      contactPersonName: [this.customer.contactPersonName],
      customerType: [this.customer.customerType, Validators.required],
      industry: [this.customer.industry],
      website: [this.customer.website],
      description: [this.customer.description]
    });
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    if (this.isEditing && !this.editForm) {
      this.initEditForm();
    }
  }

  cancelEdit() {
    this.isEditing = false;
    this.initEditForm(); // Reset form to original values
  }

  saveChanges() {
    if (!this.editForm || !this.customer || this.editForm.invalid || !this.customer.id) return;

    this.loading = true;
    const updatedData = this.editForm.value;

    this.customerService.updateCustomer(this.customer.id, updatedData).subscribe({
      next: (updatedCustomer: Customer) => {
        this.customer = updatedCustomer;
        this.isEditing = false;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error updating customer:', error);
        this.error = 'Failed to update customer';
        this.loading = false;
      }
    });
  }

  goBack() {
    this.router.navigate(['/customers']);
  }
}