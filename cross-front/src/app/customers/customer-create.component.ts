import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { CustomerService, Customer, CustomerContact, CustomerAddress } from '../services/customer.service';

@Component({
  selector: 'app-customer-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './customer-create.component.html',
  styleUrls: ['./customer-create.component.scss']
})
export class CustomerCreateComponent implements OnInit, OnDestroy {
  customerForm!: FormGroup;
  loading = false;
  error: string | null = null;

  // Form options
  customerTypes = [
    { value: 'lead', label: 'Lead (Initial Contact)' },
    { value: 'prospect', label: 'Prospect (Qualified Lead)' },
    { value: 'customer', label: 'Active Customer (Paying)' },
    { value: 'client', label: 'Established Client (Regular Buyer)' },
    { value: 'enterprise', label: 'Enterprise Client (Large Account)' },
    { value: 'partner', label: 'Business Partner' },
    { value: 'dormant', label: 'Dormant (Previous Customer)' }
  ];

  statuses = [
    { value: 'active', label: 'Active (Engaged)' },
    { value: 'purchasing', label: 'Currently Purchasing' },
    { value: 'inactive', label: 'Inactive (Not Engaged)' },
    { value: 'lost', label: 'Lost Customer' },
    { value: 'blacklisted', label: 'Blacklisted' }
  ];

  priorities = [
    { value: 'low', label: 'Low' },
    { value: 'medium', label: 'Medium' },
    { value: 'high', label: 'High' },
    { value: 'critical', label: 'Critical' }
  ];

  sources = [
    { value: 'website', label: 'Website' },
    { value: 'referral', label: 'Referral' },
    { value: 'social', label: 'Social Media' },
    { value: 'advertising', label: 'Advertising' },
    { value: 'trade_show', label: 'Trade Show' },
    { value: 'cold_call', label: 'Cold Call' },
    { value: 'other', label: 'Other' }
  ];

  contactTypes = [
    { value: 'email', label: 'Email' },
    { value: 'phone', label: 'Phone' },
    { value: 'mobile', label: 'Mobile' },
    { value: 'fax', label: 'Fax' },
    { value: 'website', label: 'Website' },
    { value: 'linkedin', label: 'LinkedIn' },
    { value: 'other', label: 'Other' }
  ];

  addressTypes = [
    { value: 'billing', label: 'Billing Address' },
    { value: 'shipping', label: 'Shipping Address' },
    { value: 'office', label: 'Office Address' },
    { value: 'warehouse', label: 'Warehouse' },
    { value: 'other', label: 'Other' }
  ];

  industries = [
    'Technology', 'Finance', 'Healthcare', 'Manufacturing', 'Retail', 'Education',
    'Real Estate', 'Construction', 'Transportation', 'Entertainment', 'Agriculture',
    'Energy', 'Telecommunications', 'Government', 'Non-Profit', 'Other'
  ];

  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService,
    private router: Router
  ) {
    this.initializeForm();
  }

  ngOnInit() {
    // Form is already initialized in constructor
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  private initializeForm() {
    this.customerForm = this.fb.group({
      // Basic Information
      companyName: ['', [Validators.required, Validators.minLength(2)]],
      contactPersonName: [''],
      customerType: ['prospect', Validators.required],
      industry: [''],
      website: [''],
      description: [''],
      source: ['website', Validators.required],
      
      // Business Information
      annualRevenue: [''],
      employeeCount: [''],
      taxId: [''],
      relationshipStartDate: [''],
      firstPurchaseDate: [''],
      customerLifetimeValue: [''],
      
      // CRM Information
      status: ['active', Validators.required],
      priority: ['medium', Validators.required],
      
      // Contact Information
      contacts: this.fb.array([]),
      
      // Address Information
      addresses: this.fb.array([])
    });

    // Add default contact and address
    this.addContact();
    this.addAddress();
  }

  // Contact Management
  get contacts() {
    return this.customerForm.get('contacts') as FormArray;
  }

  addContact() {
    const contactForm = this.fb.group({
      type: ['email', Validators.required],
      value: ['', Validators.required],
      isPrimary: [this.contacts.length === 0],
      label: ['']
    });

    this.contacts.push(contactForm);
  }

  removeContact(index: number) {
    if (this.contacts.length > 1) {
      this.contacts.removeAt(index);
      
      // Ensure at least one contact is marked as primary
      if (!this.contacts.controls.some(c => c.get('isPrimary')?.value)) {
        this.contacts.at(0).get('isPrimary')?.setValue(true);
      }
    }
  }

  setPrimaryContact(index: number) {
    this.contacts.controls.forEach((control, i) => {
      control.get('isPrimary')?.setValue(i === index);
    });
  }

  // Address Management
  get addresses() {
    return this.customerForm.get('addresses') as FormArray;
  }

  addAddress() {
    const addressForm = this.fb.group({
      type: ['billing', Validators.required],
      addressLine1: ['', Validators.required],
      addressLine2: [''],
      city: ['', Validators.required],
      state: [''],
      postalCode: [''],
      country: ['', Validators.required],
      isPrimary: [this.addresses.length === 0],
      label: ['']
    });

    this.addresses.push(addressForm);
  }

  removeAddress(index: number) {
    if (this.addresses.length > 1) {
      this.addresses.removeAt(index);
      
      // Ensure at least one address is marked as primary
      if (!this.addresses.controls.some(a => a.get('isPrimary')?.value)) {
        this.addresses.at(0).get('isPrimary')?.setValue(true);
      }
    }
  }

  setPrimaryAddress(index: number) {
    this.addresses.controls.forEach((control, i) => {
      control.get('isPrimary')?.setValue(i === index);
    });
  }

  // Form Validation
  isFieldInvalid(fieldName: string): boolean {
    const field = this.customerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.customerForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) {
        return `${this.getFieldLabel(fieldName)} is required`;
      }
      if (field.errors['minlength']) {
        return `${this.getFieldLabel(fieldName)} must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
      if (field.errors['email']) {
        return 'Please enter a valid email address';
      }
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      companyName: 'Company Name',
      contactPersonName: 'Contact Person',
      customerType: 'Customer Type',
      industry: 'Industry',
      source: 'Source',
      status: 'Status',
      priority: 'Priority'
    };
    return labels[fieldName] || fieldName;
  }

  // Form Submission
  onSubmit() {
    if (this.customerForm.invalid) {
      // Mark all fields as touched to show validation errors
      this.markFormGroupTouched(this.customerForm);
      return;
    }

    this.loading = true;
    this.error = null;

    const formValue = this.customerForm.value;

    // Prepare customer data
    const customerData: Partial<Customer> = {
      companyName: formValue.companyName,
      contactPersonName: formValue.contactPersonName || undefined,
      customerType: formValue.customerType,
      industry: formValue.industry || undefined,
      website: formValue.website || undefined,
      description: formValue.description || undefined,
      source: formValue.source,
      annualRevenue: formValue.annualRevenue ? Number(formValue.annualRevenue) : undefined,
      employeeCount: formValue.employeeCount ? Number(formValue.employeeCount) : undefined,
      taxId: formValue.taxId || undefined,
      status: formValue.status,
      priority: formValue.priority,
      contacts: formValue.contacts.filter((c: any) => c.value.trim()),
      addresses: formValue.addresses.filter((a: any) => a.addressLine1.trim())
    };

    this.subscriptions.add(
      this.customerService.createCustomer(customerData).subscribe({
        next: (customer) => {
          console.log('Customer created successfully:', customer);
          this.router.navigate(['/customers', customer.id]);
        },
        error: (error) => {
          this.error = error.message || 'Failed to create customer';
          this.loading = false;
          console.error('Error creating customer:', error);
        }
      })
    );
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
      if (control instanceof FormArray) {
        control.controls.forEach(arrayControl => {
          if (arrayControl instanceof FormGroup) {
            this.markFormGroupTouched(arrayControl);
          }
        });
      }
    });
  }

  // Navigation
  goBack() {
    this.router.navigate(['/customers']);
  }

  // Utility
  formatCurrency(value: any): string {
    if (!value) return '';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(Number(value));
  }
}